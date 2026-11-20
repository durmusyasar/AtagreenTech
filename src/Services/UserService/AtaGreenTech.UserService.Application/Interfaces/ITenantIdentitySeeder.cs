using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Application.Interfaces
{
    /// <summary>
    /// Tenant DB'sinde izin/rol kataloğunu ve varsayılan Admin kullanıcısını idempotent olarak sağlar.
    /// Mevcut tenant kapsamında (HTTP veya <c>ITenantExecutionContext</c>) çalışmalıdır.
    /// </summary>
    public interface ITenantIdentitySeeder
    {
        /// <summary>
        /// RBAC kataloğunu ve varsayılan Admin'i idempotent olarak sağlar (login bootstrap / add-tenant ortak yolu).
        /// </summary>
        /// <param name="request">İsteğe özel Admin bilgileri; null ise yapılandırılmış bootstrap kullanılır.</param>
        /// <param name="mode">Upsert veya yalnızca boş tenant'ta bootstrap.</param>
        /// <param name="cancellationToken">İptal jetonu.</param>
        Task<TenantIdentitySeedResult> EnsureAsync(
            TenantIdentitySeedRequest? request = null,
            TenantIdentitySeedMode mode = TenantIdentitySeedMode.UpsertAdmin,
            CancellationToken cancellationToken = default
        );
    }
}