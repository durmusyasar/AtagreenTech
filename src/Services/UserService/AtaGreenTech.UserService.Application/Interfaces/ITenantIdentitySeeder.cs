using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>
/// Tenant DB'sinde izin/rol kataloğunu ve varsayılan Admin kullanıcısını idempotent olarak sağlar.
/// Mevcut tenant kapsamında (HTTP veya <c>ITenantExecutionContext</c>) çalışmalıdır.
/// </summary>
public interface ITenantIdentitySeeder
{
    Task<TenantIdentitySeedResult> EnsureAsync(
        TenantIdentitySeedRequest? request = null,
        TenantIdentitySeedMode mode = TenantIdentitySeedMode.UpsertAdmin,
        CancellationToken cancellationToken = default
    );
}
