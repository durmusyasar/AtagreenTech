namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Tenant kimlik seed isteği. Boş alanlar <c>Auth:Bootstrap</c> varsayılanlarıyla doldurulur.
    /// Tenant izolasyonu DB düzeyinde olduğundan ayrı <c>TenantId</c> alanı yoktur; çağıran
    /// <see cref="OmniSync.Core.Domain.Tenants.ITenantExecutionContext"/> veya HTTP tenant host ile kapsam açar.
    /// </summary>
    public sealed record TenantIdentitySeedRequest(
        string? AdminEmail = null,
        string? AdminPassword = null,
        string? AdminUsername = null,
        string? AdminFirstName = null,
        string? AdminLastName = null
    );

    /// <summary>Seed sonucu: RBAC kataloğu her zaman sağlanır; admin yeni oluşturulmuş veya güncellenmiş olabilir.</summary>
    public sealed record TenantIdentitySeedResult(
        Guid AdminUserId,
        string AdminEmail,
        bool AdminCreated,
        string TenantIdentifier
    );

    /// <summary>
    /// <see cref="BootstrapIfEmpty"/>: yalnızca tenant'ta hiç kullanıcı yoksa admin oluşturur (login yolu).
    /// <see cref="UpsertAdmin"/>: e-postaya göre admin oluşturur veya Admin rolünü garanti eder (add-tenant / seed-tenant).
    /// </summary>
    public enum TenantIdentitySeedMode
    {
        BootstrapIfEmpty,
        UpsertAdmin,
    }

    /// <summary>
    /// Seed sırasında çözülmüş Admin kimlik alanları (<see cref="TenantIdentitySeedRequest"/> + <c>Auth:Bootstrap</c> birleşimi).
    /// </summary>
    internal sealed record ResolvedAdmin(
        string Email,
        string Password,
        string Username,
        string FirstName,
        string LastName
    );
}
