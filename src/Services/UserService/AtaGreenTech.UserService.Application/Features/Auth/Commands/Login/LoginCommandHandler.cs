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
    IRepository<RefreshToken> refreshTokens,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
) : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            throw new InvalidOperationException("Tenant could not be resolved from the host.");

        await BootstrapAdminIfNeededAsync(cancellationToken);

        var email = User.NormalizeEmail(request.Email);
        var user = await db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Geçersiz e-posta veya şifre.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Hesabınız devre dışı bırakılmıştır.");

        if (!user.IsEmailVerified)
            throw new UnauthorizedAccessException("Lütfen önce e-posta adresinizi doğrulayın.");

        var roleNames = user.UserRoles
            .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
            .Select(ur => ur.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var access = accessTokenIssuer.Create(user.Id, user.Email, currentTenant.Id.Value, roleNames);
        var (refreshPlaintext, refreshEntity) = RefreshTokenFactory.Create(
            user.Id,
            Guid.NewGuid(),
            authOptions.Value.RefreshTokenDays,
            request.ClientIp
        );

        await refreshTokens.AddAsync(refreshEntity, cancellationToken);

        return new AuthResponse(access.AccessToken, refreshPlaintext, access.ExpiresAt);
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

        var adminRole = await EnsureRoleAsync(Role.Names.Admin, "System Administrator", cancellationToken);
        await EnsureRoleAsync(Role.Names.Manager, "Tenant manager", cancellationToken);
        await EnsureRoleAsync(Role.Names.Viewer, "Read-only viewer", cancellationToken);

        var admin = await users.AddAsync(
            CreateBootstrapAdmin(bootstrap, passwordHasher),
            cancellationToken
        );
        await userRoles.AddAsync(new UserRole(admin.Id, adminRole.Id), cancellationToken);

        // Login sorgusu AsNoTracking olduğu için bootstrap kayıtlarının DB'de görünmesi gerekir.
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Role> EnsureRoleAsync(string name, string description, CancellationToken cancellationToken)
    {
        var existing = (await roles.GetAllByFilterAsync(r => r.Name == name, cancellationToken)).FirstOrDefault();
        if (existing is not null)
            return existing;

        return await roles.AddAsync(new Role(name, description), cancellationToken);
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
