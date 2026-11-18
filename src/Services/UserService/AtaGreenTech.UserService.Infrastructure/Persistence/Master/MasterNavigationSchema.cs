using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Master;

/// <summary>
/// Master DB'de <c>MenuDefinitions</c> + <c>TenantServiceSubscriptions</c> tablolarını oluşturur,
/// menü kataloğunu Code üzerinden upsert eder ve tenant'lara varsayılan abonelikleri ekler.
/// </summary>
public static class MasterNavigationSchema
{
    public static async Task EnsureAsync(MasterDbContext master, CancellationToken cancellationToken = default)
    {
        await master.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "MenuDefinitions" (
                "Id" uuid PRIMARY KEY,
                "Code" character varying(128) NOT NULL,
                "ServiceName" character varying(64) NOT NULL,
                "ServiceTranslationKey" character varying(128) NOT NULL,
                "ParentId" uuid NULL REFERENCES "MenuDefinitions" ("Id") ON DELETE SET NULL,
                "RoutePath" character varying(256) NOT NULL,
                "IconKey" character varying(64) NOT NULL,
                "TranslationKey" character varying(128) NOT NULL,
                "RequiredPermission" character varying(128) NULL,
                "DisplayOrder" integer NOT NULL,
                "ServiceDisplayOrder" integer NOT NULL,
                "IsActive" boolean NOT NULL DEFAULT TRUE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_MenuDefinitions_Code" ON "MenuDefinitions" ("Code");
            CREATE INDEX IF NOT EXISTS "IX_MenuDefinitions_ServiceName" ON "MenuDefinitions" ("ServiceName");

            CREATE TABLE IF NOT EXISTS "TenantServiceSubscriptions" (
                "Id" uuid PRIMARY KEY,
                "TenantId" uuid NOT NULL REFERENCES "Tenants" ("Id") ON DELETE CASCADE,
                "ServiceName" character varying(64) NOT NULL,
                "IsEnabled" boolean NOT NULL,
                "SubscribedAtUtc" timestamp with time zone NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TenantServiceSubscriptions_Tenant_Service"
                ON "TenantServiceSubscriptions" ("TenantId", "ServiceName");
            """,
            cancellationToken
        );

        foreach (var row in Catalog)
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "MenuDefinitions" (
                    "Id", "Code", "ServiceName", "ServiceTranslationKey", "ParentId",
                    "RoutePath", "IconKey", "TranslationKey", "RequiredPermission",
                    "DisplayOrder", "ServiceDisplayOrder", "IsActive"
                ) VALUES (
                    {row.Id}, {row.Code}, {row.ServiceName}, {row.ServiceTranslationKey}, NULL,
                    {row.RoutePath}, {row.IconKey}, {row.TranslationKey}, {row.RequiredPermission},
                    {row.DisplayOrder}, {row.ServiceDisplayOrder}, TRUE
                )
                ON CONFLICT ("Code") DO UPDATE SET
                    "ServiceName" = EXCLUDED."ServiceName",
                    "ServiceTranslationKey" = EXCLUDED."ServiceTranslationKey",
                    "RoutePath" = EXCLUDED."RoutePath",
                    "IconKey" = EXCLUDED."IconKey",
                    "TranslationKey" = EXCLUDED."TranslationKey",
                    "RequiredPermission" = EXCLUDED."RequiredPermission",
                    "DisplayOrder" = EXCLUDED."DisplayOrder",
                    "ServiceDisplayOrder" = EXCLUDED."ServiceDisplayOrder",
                    "IsActive" = EXCLUDED."IsActive";
                """,
                cancellationToken
            );
        }

        // General / UserService / System her tenant'ta açık; StockService varsayılan açık (lisans ile kapatılabilir).
        await master.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "TenantServiceSubscriptions" ("Id", "TenantId", "ServiceName", "IsEnabled", "SubscribedAtUtc")
            SELECT gen_random_uuid(), t."Id", s."ServiceName", s."IsEnabled", now()
            FROM "Tenants" t
            CROSS JOIN (VALUES
                ('General', TRUE),
                ('UserService', TRUE),
                ('StockService', TRUE),
                ('System', TRUE)
            ) AS s("ServiceName", "IsEnabled")
            WHERE NOT t."IsDeleted"
              AND NOT EXISTS (
                  SELECT 1 FROM "TenantServiceSubscriptions" x
                  WHERE x."TenantId" = t."Id" AND x."ServiceName" = s."ServiceName"
              );
            """,
            cancellationToken
        );
    }

    /// <summary>Idempotent seed kaynağı — Code sabit, Id deterministik.</summary>
    internal static IReadOnlyList<MenuSeedRow> Catalog { get; } =
    [
        new(Guid.Parse("11111111-1111-1111-1111-111111110001"), "general.dashboard", "General", "services.general", "/", "dashboard", "menu.dashboard", null, 10, 0),
        new(Guid.Parse("11111111-1111-1111-1111-111111110101"), "stock.units", "StockService", "services.stock", "/units-of-measure", "units", "menu.unitsOfMeasure", "stock.read", 10, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110102"), "stock.categories", "StockService", "services.stock", "/categories", "categories", "menu.categories", "stock.read", 20, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110103"), "stock.products", "StockService", "services.stock", "/products", "products", "menu.products", "stock.read", 30, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110104"), "stock.warehouses", "StockService", "services.stock", "/warehouses", "warehouses", "menu.warehouses", "stock.read", 40, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110105"), "stock.movements", "StockService", "services.stock", "/stock-movements", "movements", "menu.stockMovements", "stock.read", 50, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110106"), "stock.balances", "StockService", "services.stock", "/stock-balances", "balances", "menu.stockBalances", "stock.read", 60, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110201"), "iam.security", "UserService", "services.iam", "/account/security", "security", "menu.security", null, 10, 20),
        new(Guid.Parse("11111111-1111-1111-1111-111111110202"), "iam.users", "UserService", "services.iam", "/admin/users", "users", "menu.users", "users.read", 20, 20),
        new(Guid.Parse("11111111-1111-1111-1111-111111110203"), "iam.roles", "UserService", "services.iam", "/admin/roles", "roles", "menu.roles", "users.read", 30, 20),
        new(Guid.Parse("11111111-1111-1111-1111-111111110301"), "system.overview", "System", "services.system", "/admin/system", "system", "menu.system", "system.read", 10, 30),
    ];

    internal sealed record MenuSeedRow(
        Guid Id,
        string Code,
        string ServiceName,
        string ServiceTranslationKey,
        string RoutePath,
        string IconKey,
        string TranslationKey,
        string? RequiredPermission,
        int DisplayOrder,
        int ServiceDisplayOrder
    );
}
