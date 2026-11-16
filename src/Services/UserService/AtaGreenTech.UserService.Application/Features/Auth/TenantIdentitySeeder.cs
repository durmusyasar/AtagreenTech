using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// <see cref="RbacCatalog"/> + varsayılan Admin seed. Login bootstrap ve add-tenant CLI/API ortak yolu.
/// </summary>
public sealed class TenantIdentitySeeder(
    IRepository<User> users,
    IRepository<Role> roles,
    IRepository<UserRole> userRoles,
    IRepository<Permission> permissions,
    IRepository<RolePermission> rolePermissions,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IIdentityAuditLogger auditLogger,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
) : ITenantIdentitySeeder
{
    public async Task<TenantIdentitySeedResult> EnsureAsync(
        TenantIdentitySeedRequest? request = null,
        TenantIdentitySeedMode mode = TenantIdentitySeedMode.UpsertAdmin,
        CancellationToken cancellationToken = default
    )
    {
        if (!currentTenant.IsAvailable || string.IsNullOrWhiteSpace(currentTenant.Identifier))
            throw new InvalidOperationException("Tenant could not be resolved for identity seed.");

        var tenantIdentifier = currentTenant.Identifier;
        var bootstrap = authOptions.Value.Bootstrap;
        var resolved = Resolve(request, bootstrap);

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, unitOfWork, cancellationToken);

        var adminRole = (await roles.GetAllByFilterAsync(r => r.Name == Role.Names.Admin, cancellationToken))
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Admin role was not seeded.");

        var email = User.NormalizeEmail(resolved.Email);
        var existing = await users.GetAsync(u => u.Email == email, cancellationToken);

        if (existing is not null)
        {
            await EnsureAdminRoleAsync(existing.Id, adminRole.Id, cancellationToken);
            return new TenantIdentitySeedResult(existing.Id, existing.Email, AdminCreated: false, tenantIdentifier);
        }

        if (mode == TenantIdentitySeedMode.BootstrapIfEmpty && await users.ExistsAsync(_ => true, cancellationToken))
        {
            // Login yolu: tenant zaten kullanıcı içeriyor; bootstrap atlanır.
            return new TenantIdentitySeedResult(Guid.Empty, email, AdminCreated: false, tenantIdentifier);
        }

        if (string.IsNullOrWhiteSpace(resolved.Password))
        {
            throw new InvalidOperationException(
                "Tenant has no users and Auth:Bootstrap:Password is not configured."
            );
        }

        var (passwordOk, passwordMessage) = PasswordRules.Validate(resolved.Password);
        if (!passwordOk)
            throw new ArgumentException($"Geçersiz başlangıç şifresi: {passwordMessage}", nameof(request));

        var admin = new User(
            email,
            resolved.Username,
            passwordHasher.Hash(resolved.Password),
            resolved.FirstName,
            resolved.LastName
        );
        admin.MarkEmailVerified();

        admin = await users.AddAsync(admin, cancellationToken);
        await userRoles.AddAsync(new UserRole(admin.Id, adminRole.Id), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            AuditLog.Actions.TenantIdentitySeeded,
            admin.Email,
            admin.Id,
            details: $"Tenant '{tenantIdentifier}' için varsayılan Admin ve RBAC kataloğu seed edildi.",
            cancellationToken: cancellationToken
        );
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantIdentitySeedResult(admin.Id, admin.Email, AdminCreated: true, tenantIdentifier);
    }

    private async Task EnsureAdminRoleAsync(Guid userId, Guid adminRoleId, CancellationToken cancellationToken)
    {
        var hasAdmin = await userRoles.ExistsAsync(
            ur => ur.UserId == userId && ur.RoleId == adminRoleId,
            cancellationToken
        );
        if (hasAdmin)
            return;

        await userRoles.AddAsync(new UserRole(userId, adminRoleId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ResolvedAdmin Resolve(TenantIdentitySeedRequest? request, BootstrapAdminOptions bootstrap)
    {
        var email = FirstNonEmpty(request?.AdminEmail, bootstrap.Email)
            ?? throw new ArgumentException("Admin e-posta zorunludur.");
        var password = FirstNonEmpty(request?.AdminPassword, bootstrap.Password) ?? string.Empty;
        var username = FirstNonEmpty(request?.AdminUsername, bootstrap.Username) ?? "admin";
        var firstName = FirstNonEmpty(request?.AdminFirstName, bootstrap.FirstName) ?? "System";
        var lastName = FirstNonEmpty(request?.AdminLastName, bootstrap.LastName) ?? "Admin";
        return new ResolvedAdmin(email, password, username, firstName, lastName);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

    private sealed record ResolvedAdmin(
        string Email,
        string Password,
        string Username,
        string FirstName,
        string LastName
    );
}
