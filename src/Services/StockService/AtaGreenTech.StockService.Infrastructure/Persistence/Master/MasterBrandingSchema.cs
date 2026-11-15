using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Master;

/// <summary>
/// OmniSync master migration'larından sonra <c>TenantBrandings</c> tablosunu oluşturur ve mevcut tenant'lar
/// için varsayılan satırları ekler. OmniSync <c>Tenant</c> modeline alan eklemez.
/// </summary>
public static class MasterBrandingSchema
{
    public static async Task EnsureAsync(MasterDbContext master, CancellationToken cancellationToken = default)
    {
        await master.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "TenantBrandings" (
                "TenantId" uuid PRIMARY KEY REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                "DisplayName" character varying(150) NULL,
                "LogoUrl" character varying(2048) NULL,
                "FaviconUrl" character varying(2048) NULL,
                "PrimaryColor" character varying(7) NOT NULL,
                "UpdatedAt" timestamp with time zone NULL
            );
            """,
            cancellationToken
        );

        // Kayıtlı her aktif tenant için varsayılan marka satırı; mevcut satırlar dokunulmaz.
        await master.Database.ExecuteSqlRawAsync(
            $"""
            INSERT INTO "TenantBrandings" ("TenantId", "DisplayName", "LogoUrl", "FaviconUrl", "PrimaryColor", "UpdatedAt")
            SELECT t."Id", t."Name", '{TenantBrandingDefaults.LogoUrl}', '{TenantBrandingDefaults.FaviconUrl}', '{TenantBrandingDefaults.PrimaryColor}', now()
            FROM "Tenants" t
            WHERE NOT t."IsDeleted"
              AND NOT EXISTS (SELECT 1 FROM "TenantBrandings" b WHERE b."TenantId" = t."Id");
            """,
            cancellationToken
        );
    }
}
