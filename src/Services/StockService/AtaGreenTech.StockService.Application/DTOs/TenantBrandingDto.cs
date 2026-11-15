namespace AtaGreenTech.StockService.Application.DTOs;

/// <summary>
/// Aktif tenant'ın marka ayarları. Login dahil herkese açık <c>GET /api/v1/tenant/config</c> yanıtıdır.
/// </summary>
public sealed record TenantBrandingDto(
    Guid TenantId,
    string Subdomain,
    string Name,
    string PrimaryColor,
    string? LogoUrl,
    string? FaviconUrl
);
