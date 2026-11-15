using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Master;

public sealed class TenantBrandingReader(MasterDbContext master) : ITenantBrandingReader
{
    public async Task<TenantBrandingDto?> GetByTenantIdAsync(
        Guid tenantId,
        string subdomain,
        string tenantName,
        CancellationToken cancellationToken = default
    )
    {
        var row = await master.Database
            .SqlQuery<BrandingRow>(
                $"""
                SELECT
                    b."TenantId" AS "TenantId",
                    b."DisplayName" AS "DisplayName",
                    b."LogoUrl" AS "LogoUrl",
                    b."FaviconUrl" AS "FaviconUrl",
                    b."PrimaryColor" AS "PrimaryColor"
                FROM "TenantBrandings" b
                WHERE b."TenantId" = {tenantId}
                """
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return new TenantBrandingDto(
                tenantId,
                subdomain,
                tenantName,
                TenantBrandingDefaults.PrimaryColor,
                TenantBrandingDefaults.LogoUrl,
                TenantBrandingDefaults.FaviconUrl
            );
        }

        return new TenantBrandingDto(
            row.TenantId,
            subdomain,
            string.IsNullOrWhiteSpace(row.DisplayName) ? tenantName : row.DisplayName.Trim(),
            string.IsNullOrWhiteSpace(row.PrimaryColor) ? TenantBrandingDefaults.PrimaryColor : row.PrimaryColor.Trim(),
            NullIfWhiteSpace(row.LogoUrl) ?? TenantBrandingDefaults.LogoUrl,
            NullIfWhiteSpace(row.FaviconUrl) ?? TenantBrandingDefaults.FaviconUrl
        );
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record BrandingRow(
        Guid TenantId,
        string? DisplayName,
        string? LogoUrl,
        string? FaviconUrl,
        string PrimaryColor
    );
}
