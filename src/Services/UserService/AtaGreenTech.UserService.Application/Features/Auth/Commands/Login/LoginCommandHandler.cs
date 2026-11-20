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
    IRepository<RefreshToken> refreshTokens,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    ITotpService totpService,
    IIdentityAuditLogger auditLogger,
    ICurrentTenant currentTenant,
    ITenantIdentitySeeder identitySeeder,
    IOptions<AuthOptions> authOptions,
    AccessTokenLifecycle accessTokenLifecycle
) : ICommandHandler<LoginCommand, AuthAttemptResult>
{
    public async Task<AuthAttemptResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            throw new InvalidOperationException("Tenant could not be resolved from the host.");

        await identitySeeder.EnsureAsync(
            mode: TenantIdentitySeedMode.BootstrapIfEmpty,
            cancellationToken: cancellationToken
        );

        var email = User.NormalizeEmail(request.Email);
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

        if (user.IsMfaEnabled)
        {
            var mfaResult = await VerifyMfaAsync(user, request, options, utcNow, cancellationToken);
            if (mfaResult is not null)
                return mfaResult;
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

        var sessionId = Guid.NewGuid();
        var access = accessTokenIssuer.Create(
            user.Id,
            user.Email,
            currentTenant.Id.Value,
            sessionId,
            user.SecurityStamp,
            roleNames,
            permissionNames
        );
        var (refreshPlaintext, refreshEntity) = RefreshTokenFactory.Create(
            user.Id,
            sessionId,
            options.RefreshTokenDays,
            request.ClientIp,
            request.DeviceInfo
        );

        await refreshTokens.AddAsync(refreshEntity, cancellationToken);
        await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);
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

    private async Task<AuthAttemptResult?> VerifyMfaAsync(
        User user,
        LoginCommand request,
        AuthOptions options,
        DateTime utcNow,
        CancellationToken cancellationToken
    )
    {
        var hasTotp = !string.IsNullOrWhiteSpace(request.TotpCode);
        var hasRecovery = !string.IsNullOrWhiteSpace(request.RecoveryCode);

        if (!hasTotp && !hasRecovery)
            return AuthAttemptResult.RequireMfa();

        var valid = false;
        if (hasTotp)
        {
            valid = !string.IsNullOrWhiteSpace(user.TotpSecret)
                    && totpService.VerifyCode(user.TotpSecret, request.TotpCode!);
        }
        else if (hasRecovery)
        {
            var hash = RecoveryCodeFactory.Hash(request.RecoveryCode!);
            valid = user.TryConsumeRecoveryCodeHash(hash);
        }

        if (valid)
            return null;

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
            "Geçersiz MFA veya kurtarma kodu.",
            cancellationToken
        );
        return AuthAttemptResult.Fail(
            AuthErrorCodes.MfaInvalid,
            "Geçersiz MFA veya kurtarma kodu."
        );
    }
}
