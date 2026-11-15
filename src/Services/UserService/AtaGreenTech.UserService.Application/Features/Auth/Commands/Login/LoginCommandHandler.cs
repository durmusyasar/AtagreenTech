using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserDbContext db,
    IRepository<User> users,
    IRepository<Role> roles,
    IRepository<UserRole> userRoles,
    IRepository<Permission> permissions,
    IRepository<RolePermission> rolePermissions,
    IRepository<RefreshToken> refreshTokens,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    IIdentityAuditLogger auditLogger,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
) : ICommandHandler<LoginCommand, AuthAttemptResult>
{
    public async Task<AuthAttemptResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            throw new InvalidOperationException("Tenant could not be resolved from the host.");

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, unitOfWork, cancellationToken);
        await BootstrapAdminIfNeededAsync(cancellationToken);

        var email = User.NormalizeEmail(request.Email);
        // Tracked entity: lockout sayacı transaction commit ile kalıcı olmalı.
        var user = await users.GetAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            await auditLogger.LogAsync(
                AuditLog.Actions.LoginFailure,
                email,
                ipAddress: request.ClientIp,
                details: "Geçersiz e-posta veya şifre.",
                cancellationToken: cancellationToken
            );
            return AuthAttemptResult.Fail(
                AuthErrorCodes.InvalidCredentials,
                "Geçersiz e-posta veya şifre."
            );
        }

        var utcNow = DateTime.UtcNow;
        var options = authOptions.Value;

        if (user.IsLockedOut(utcNow))
        {
            var minutesLeft = (int)Math.Ceiling((user.LockoutEnd!.Value - utcNow).TotalMinutes);
            await auditLogger.LogAsync(
                AuditLog.Actions.LoginFailure,
                user.Email,
                user.Id,
                request.ClientIp,
                "Hesap kilitli.",
                cancellationToken
            );
            return AuthAttemptResult.Fail(
                AuthErrorCodes.AccountLocked,
                $"Çok fazla başarısız deneme. Lütfen {minutesLeft} dakika sonra tekrar deneyin.",
                statusCode: 423
            );
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedAccess(
                options.MaxFailedAccessAttempts,
                TimeSpan.FromMinutes(options.LockoutMinutes),
                utcNow
            );
            await auditLogger.LogAsync(
                AuditLog.Actions.LoginFailure,
                user.Email,
                user.Id,
                request.ClientIp,
                "Hatalı şifre.",
                cancellationToken
            );
            return AuthAttemptResult.Fail(
                AuthErrorCodes.InvalidCredentials,
                "Geçersiz e-posta veya şifre."
            );
        }

        if (!user.IsActive)
        {
            await auditLogger.LogAsync(
                AuditLog.Actions.LoginFailure,
                user.Email,
                user.Id,
                request.ClientIp,
                "Hesap pasif.",
                cancellationToken
            );
            return AuthAttemptResult.Fail(
                AuthErrorCodes.AccountInactive,
                "Hesabınız devre dışı bırakılmıştır.",
                statusCode: 403
            );
        }

        if (!user.IsEmailVerified)
        {
            await auditLogger.LogAsync(
                AuditLog.Actions.LoginFailure,
                user.Email,
                user.Id,
                request.ClientIp,
                "E-posta doğrulanmamış.",
                cancellationToken
            );
            return AuthAttemptResult.Fail(
                AuthErrorCodes.EmailNotVerified,
                "Lütfen önce e-posta adresinizi doğrulayın.",
                statusCode: 403
            );
        }

        user.ResetAccessFailures();

        var userWithRoles = await db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id, cancellationToken);

        var roleNames = userWithRoles.UserRoles
            .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
            .Select(ur => ur.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissionNames = await UserPermissionResolver.ResolveAsync(db, user.Id, cancellationToken);

        var access = accessTokenIssuer.Create(
            user.Id,
            user.Email,
            currentTenant.Id.Value,
            roleNames,
            permissionNames
        );
        var (refreshPlaintext, refreshEntity) = RefreshTokenFactory.Create(
            user.Id,
            Guid.NewGuid(),
            options.RefreshTokenDays,
            request.ClientIp,
            request.DeviceInfo
        );

        await refreshTokens.AddAsync(refreshEntity, cancellationToken);
        await auditLogger.LogAsync(
            AuditLog.Actions.LoginSuccess,
            user.Email,
            user.Id,
            request.ClientIp,
            "Kullanıcı sisteme başarıyla giriş yaptı.",
            cancellationToken
        );

        return AuthAttemptResult.Ok(new AuthResponse(access.AccessToken, refreshPlaintext, access.ExpiresAt));
    }

    private async Task BootstrapAdminIfNeededAsync(CancellationToken cancellationToken)
    {
        if (await users.ExistsAsync(_ => true, cancellationToken))
            return;

        var bootstrap = authOptions.Value.Bootstrap;
        if (string.IsNullOrWhiteSpace(bootstrap.Password))
        {
            throw new InvalidOperationException(
                "Tenant has no users and Auth:Bootstrap:Password is not configured."
            );
        }

        var adminRole = (await roles.GetAllByFilterAsync(r => r.Name == Role.Names.Admin, cancellationToken))
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Admin role was not seeded.");

        var admin = await users.AddAsync(
            CreateBootstrapAdmin(bootstrap, passwordHasher),
            cancellationToken
        );
        await userRoles.AddAsync(new UserRole(admin.Id, adminRole.Id), cancellationToken);

        // Login sorgusu AsNoTracking / ayrı GetAsync olduğu için bootstrap'ın DB'de görünmesi gerekir.
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static User CreateBootstrapAdmin(BootstrapAdminOptions bootstrap, IPasswordHasher passwordHasher)
    {
        var admin = new User(
            bootstrap.Email,
            bootstrap.Username,
            passwordHasher.Hash(bootstrap.Password),
            bootstrap.FirstName,
            bootstrap.LastName
        );
        admin.MarkEmailVerified();
        return admin;
    }
}
