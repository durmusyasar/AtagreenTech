using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Common;
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
    IOptions<AuthOptions> authOptions,
    IErrorLocalizer localizer
) : ITenantIdentitySeeder
{
    /// <summary>
    /// Mevcut tenant kapsamında RBAC kataloğunu sağlar; gerekirse varsayılan Admin kullanıcısını oluşturur
    /// veya mevcut Admin'e rol atar. Login bootstrap ile add-tenant CLI/API ortak giriş noktasıdır.
    /// </summary>
    /// <param name="request">İstek özelinde Admin kimlik bilgileri; null ise <c>Auth:Bootstrap</c> kullanılır.</param>
    /// <param name="mode">
    /// <see cref="TenantIdentitySeedMode.UpsertAdmin"/> her zaman Admin'i sağlar;
    /// <see cref="TenantIdentitySeedMode.BootstrapIfEmpty"/> yalnızca tenant boşken oluşturur.
    /// </param>
    /// <param name="cancellationToken">İptal jetonu.</param>
    /// <returns>Oluşturulan veya eşleşen Admin özeti (<see cref="TenantIdentitySeedResult.AdminCreated"/> ile).</returns>
    /// <exception cref="InvalidOperationException">Tenant çözülemezse, Admin rolü yoksa veya bootstrap parolası eksikse.</exception>
    /// <exception cref="ArgumentException">Çözülen başlangıç parolası <see cref="PasswordRules"/> ile uyumsuzsa.</exception>
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

        var (passwordOk, passwordErrorCode, _) = PasswordRules.Validate(resolved.Password);
        if (!passwordOk)
        {
            // CLI/ops teşhis: kullanıcıya giden lokalize metin değil; kararlı ErrorCode taşır.
            throw new ArgumentException(passwordErrorCode, nameof(request));
        }

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
            details: localizer.Localize(ErrorCodes.Audit.TenantIdentitySeeded, fallback: null, tenantIdentifier),
            cancellationToken: cancellationToken
        );
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantIdentitySeedResult(admin.Id, admin.Email, AdminCreated: true, tenantIdentifier);
    }

    /// <summary>
    /// Kullanıcıda Admin rolü yoksa atar; zaten varsa no-op (idempotent).
    /// </summary>
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

    /// <summary>
    /// İstek alanlarını bootstrap seçenekleriyle birleştirir; boş olmayan ilk değeri tercih eder.
    /// E-posta zorunludur; parola boş kalabilir (<see cref="TenantIdentitySeedMode.BootstrapIfEmpty"/> erken çıkışında).
    /// </summary>
    private static ResolvedAdmin Resolve(TenantIdentitySeedRequest? request, BootstrapAdminOptions bootstrap)
    {
        var email = StringHelpers.FirstNonEmpty(request?.AdminEmail, bootstrap.Email)
            ?? throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(request));
        var password = StringHelpers.FirstNonEmpty(request?.AdminPassword, bootstrap.Password) ?? string.Empty;
        var username = StringHelpers.FirstNonEmpty(request?.AdminUsername, bootstrap.Username) ?? "admin";
        var firstName = StringHelpers.FirstNonEmpty(request?.AdminFirstName, bootstrap.FirstName) ?? "System";
        var lastName = StringHelpers.FirstNonEmpty(request?.AdminLastName, bootstrap.LastName) ?? "Admin";
        return new ResolvedAdmin(email, password, username, firstName, lastName);
    }
}
