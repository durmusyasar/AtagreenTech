using AtaGreenTech.StockService.Application.DTOs;

namespace AtaGreenTech.StockService.Application.Interfaces;

/// <summary>
/// Master veritabanından tenant marka ayarlarını okur. OmniSync <c>Tenant</c> entity'sine dokunmadan
/// ayrı <c>TenantBrandings</c> tablosunu kullanır.
/// </summary>
public interface ITenantBrandingReader
{
    /// <summary>
    /// Tenant için kayıtlı markayı döndürür; yoksa ad ve varsayılan renklerle bir özet üretir.
    /// </summary>
    Task<TenantBrandingDto?> GetByTenantIdAsync(Guid tenantId, string subdomain, string tenantName, CancellationToken cancellationToken = default);
}
