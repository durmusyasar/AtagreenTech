using AtaGreenTech.UserService.Domain.Navigation;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Master;

/// <summary>
/// Master DB navigasyon şeması: Languages / Modules / çeviri tabloları + MenuDefinitions + abonelikler.
/// </summary>
public static class MasterNavigationSchema
{
    internal static readonly Guid LanguageTrId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111110001");
    internal static readonly Guid LanguageEnId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111110002");

    internal static readonly Guid ModuleGeneralId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111110001");
    internal static readonly Guid ModuleStockId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111110002");
    internal static readonly Guid ModuleIamId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111110003");
    internal static readonly Guid ModuleSystemId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111110004");

    public static async Task EnsureAsync(MasterDbContext master, CancellationToken cancellationToken = default)
    {
        await CreateBaseTablesAsync(master, cancellationToken);
        await MigrateMenuDefinitionsAsync(master, cancellationToken);
        await SeedLanguagesAsync(master, cancellationToken);
        await SeedModulesAsync(master, cancellationToken);
        await SeedMenusAsync(master, cancellationToken);
        await SeedTranslationsAsync(master, cancellationToken);
        await SeedTenantSubscriptionsAsync(master, cancellationToken);
        await DropLegacyColumnsAsync(master, cancellationToken);
    }

    private static async Task CreateBaseTablesAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        await master.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "Languages" (
                "Id" uuid PRIMARY KEY,
                "Code" character varying(16) NOT NULL,
                "Name" character varying(64) NOT NULL,
                "IsDefault" boolean NOT NULL DEFAULT FALSE,
                "IsActive" boolean NOT NULL DEFAULT TRUE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Languages_Code" ON "Languages" ("Code");

            CREATE TABLE IF NOT EXISTS "Modules" (
                "Id" uuid PRIMARY KEY,
                "Code" character varying(32) NOT NULL,
                "ServiceName" character varying(64) NOT NULL,
                "IconKey" character varying(64) NOT NULL,
                "DisplayOrder" integer NOT NULL,
                "IsCore" boolean NOT NULL DEFAULT FALSE,
                "IsActive" boolean NOT NULL DEFAULT TRUE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Modules_Code" ON "Modules" ("Code");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_Modules_ServiceName" ON "Modules" ("ServiceName");

            CREATE TABLE IF NOT EXISTS "ModuleLanguages" (
                "Id" uuid PRIMARY KEY,
                "ModuleId" uuid NOT NULL REFERENCES "Modules" ("Id") ON DELETE CASCADE,
                "LanguageId" uuid NOT NULL REFERENCES "Languages" ("Id") ON DELETE CASCADE,
                "Name" character varying(128) NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ModuleLanguages_Module_Language"
                ON "ModuleLanguages" ("ModuleId", "LanguageId");

            CREATE TABLE IF NOT EXISTS "MenuDefinitions" (
                "Id" uuid PRIMARY KEY,
                "Code" character varying(128) NOT NULL,
                "ModuleId" uuid NULL,
                "ParentId" uuid NULL,
                "RoutePath" character varying(256) NOT NULL DEFAULT '',
                "IconKey" character varying(64) NOT NULL DEFAULT '',
                "RequiredPermission" character varying(128) NULL,
                "DisplayOrder" integer NOT NULL DEFAULT 0,
                "IsActive" boolean NOT NULL DEFAULT TRUE,
                "ServiceName" character varying(64) NULL,
                "ServiceTranslationKey" character varying(128) NULL,
                "TranslationKey" character varying(128) NULL,
                "ServiceDisplayOrder" integer NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_MenuDefinitions_Code" ON "MenuDefinitions" ("Code");

            CREATE TABLE IF NOT EXISTS "MenuLanguages" (
                "Id" uuid PRIMARY KEY,
                "MenuId" uuid NOT NULL REFERENCES "MenuDefinitions" ("Id") ON DELETE CASCADE,
                "LanguageId" uuid NOT NULL REFERENCES "Languages" ("Id") ON DELETE CASCADE,
                "Name" character varying(128) NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_MenuLanguages_Menu_Language"
                ON "MenuLanguages" ("MenuId", "LanguageId");

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
    }

    private static async Task MigrateMenuDefinitionsAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        // Yalnızca eski şemadan geçişte legacy kolonları geçici olarak ekle; modern şemada yeniden yaratma.
        await master.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE "MenuDefinitions" ADD COLUMN IF NOT EXISTS "ModuleId" uuid NULL;

            DO $$
            BEGIN
              IF EXISTS (
                  SELECT 1 FROM information_schema.columns
                  WHERE table_name = 'MenuDefinitions' AND column_name = 'ServiceName'
              ) OR NOT EXISTS (
                  SELECT 1 FROM information_schema.columns
                  WHERE table_name = 'MenuDefinitions' AND column_name = 'ModuleId'
                    AND is_nullable = 'NO'
              ) THEN
                  ALTER TABLE "MenuDefinitions" ADD COLUMN IF NOT EXISTS "ServiceName" character varying(64) NULL;
                  ALTER TABLE "MenuDefinitions" ADD COLUMN IF NOT EXISTS "ServiceTranslationKey" character varying(128) NULL;
                  ALTER TABLE "MenuDefinitions" ADD COLUMN IF NOT EXISTS "TranslationKey" character varying(128) NULL;
                  ALTER TABLE "MenuDefinitions" ADD COLUMN IF NOT EXISTS "ServiceDisplayOrder" integer NULL;
              END IF;
            END $$;
            """,
            cancellationToken
        );
    }

    private static async Task SeedLanguagesAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        await master.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "Languages" ("Id", "Code", "Name", "IsDefault", "IsActive") VALUES
                ({LanguageTrId}, 'tr', 'Türkçe', TRUE, TRUE),
                ({LanguageEnId}, 'en', 'English', FALSE, TRUE)
            ON CONFLICT ("Code") DO UPDATE SET
                "Name" = EXCLUDED."Name",
                "IsDefault" = EXCLUDED."IsDefault",
                "IsActive" = EXCLUDED."IsActive";
            """,
            cancellationToken
        );
    }

    private static async Task SeedModulesAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        foreach (var module in Modules)
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "Modules" ("Id", "Code", "ServiceName", "IconKey", "DisplayOrder", "IsCore", "IsActive")
                VALUES ({module.Id}, {module.Code}, {module.ServiceName}, {module.IconKey}, {module.DisplayOrder}, {module.IsCore}, TRUE)
                ON CONFLICT ("Code") DO UPDATE SET
                    "ServiceName" = EXCLUDED."ServiceName",
                    "IconKey" = EXCLUDED."IconKey",
                    "DisplayOrder" = EXCLUDED."DisplayOrder",
                    "IsCore" = EXCLUDED."IsCore",
                    "IsActive" = EXCLUDED."IsActive";
                """,
                cancellationToken
            );
        }
    }

    private static async Task SeedMenusAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        foreach (var row in Catalog)
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "MenuDefinitions" (
                    "Id", "Code", "ModuleId", "ParentId", "RoutePath", "IconKey",
                    "RequiredPermission", "DisplayOrder", "IsActive"
                )
                SELECT {row.Id}, {row.Code}, mod."Id", NULL,
                       {row.RoutePath}, {row.IconKey}, {row.RequiredPermission},
                       {row.DisplayOrder}, TRUE
                FROM "Modules" mod
                WHERE mod."Code" = {row.ModuleCode}
                ON CONFLICT ("Code") DO UPDATE SET
                    "ModuleId" = EXCLUDED."ModuleId",
                    "RoutePath" = EXCLUDED."RoutePath",
                    "IconKey" = EXCLUDED."IconKey",
                    "RequiredPermission" = EXCLUDED."RequiredPermission",
                    "DisplayOrder" = EXCLUDED."DisplayOrder",
                    "IsActive" = EXCLUDED."IsActive";
                """,
                cancellationToken
            );
        }

        // Eski satırlarda ModuleId boşsa ServiceName üzerinden doldur (geçiş).
        await master.Database.ExecuteSqlRawAsync(
            """
            UPDATE "MenuDefinitions" m
            SET "ModuleId" = mod."Id"
            FROM "Modules" mod
            WHERE m."ModuleId" IS NULL
              AND m."ServiceName" IS NOT NULL
              AND m."ServiceName" = mod."ServiceName";
            """,
            cancellationToken
        );
    }

    private static async Task SeedTranslationsAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        foreach (var (moduleCode, tr, en) in ModuleNames)
        {
            await UpsertModuleLanguageAsync(master, moduleCode, "tr", tr, cancellationToken);
            await UpsertModuleLanguageAsync(master, moduleCode, "en", en, cancellationToken);
        }

        foreach (var (menuCode, tr, en) in MenuNames)
        {
            await UpsertMenuLanguageAsync(master, menuCode, "tr", tr, cancellationToken);
            await UpsertMenuLanguageAsync(master, menuCode, "en", en, cancellationToken);
        }
    }

    private static async Task UpsertModuleLanguageAsync(
        MasterDbContext master,
        string moduleCode,
        string languageCode,
        string name,
        CancellationToken cancellationToken
    )
    {
        var id = DeterministicIdFromCodes(moduleCode, languageCode);
        await master.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "ModuleLanguages" ("Id", "ModuleId", "LanguageId", "Name")
            SELECT {id}, mod."Id", l."Id", {name}
            FROM "Modules" mod
            CROSS JOIN "Languages" l
            WHERE mod."Code" = {moduleCode} AND l."Code" = {languageCode}
            ON CONFLICT ("ModuleId", "LanguageId") DO UPDATE SET "Name" = EXCLUDED."Name";
            """,
            cancellationToken
        );
    }

    private static async Task UpsertMenuLanguageAsync(
        MasterDbContext master,
        string menuCode,
        string languageCode,
        string name,
        CancellationToken cancellationToken
    )
    {
        var id = DeterministicIdFromCodes(menuCode, languageCode);
        await master.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "MenuLanguages" ("Id", "MenuId", "LanguageId", "Name")
            SELECT {id}, m."Id", l."Id", {name}
            FROM "MenuDefinitions" m
            CROSS JOIN "Languages" l
            WHERE m."Code" = {menuCode} AND l."Code" = {languageCode}
            ON CONFLICT ("MenuId", "LanguageId") DO UPDATE SET "Name" = EXCLUDED."Name";
            """,
            cancellationToken
        );
    }

    private static async Task SeedTenantSubscriptionsAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
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

    private static async Task DropLegacyColumnsAsync(MasterDbContext master, CancellationToken cancellationToken)
    {
        await master.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
              IF EXISTS (
                  SELECT 1 FROM information_schema.columns
                  WHERE table_name = 'MenuDefinitions' AND column_name = 'ModuleId'
              ) THEN
                  -- ModuleId zorunlu hale getir (orphan yoksa)
                  UPDATE "MenuDefinitions" SET "ModuleId" = (
                      SELECT "Id" FROM "Modules" WHERE "Code" = 'GENERAL' LIMIT 1
                  ) WHERE "ModuleId" IS NULL;

                  ALTER TABLE "MenuDefinitions" ALTER COLUMN "ModuleId" SET NOT NULL;

                  IF NOT EXISTS (
                      SELECT 1 FROM pg_constraint WHERE conname = 'FK_MenuDefinitions_Modules'
                  ) THEN
                      ALTER TABLE "MenuDefinitions"
                          ADD CONSTRAINT "FK_MenuDefinitions_Modules"
                          FOREIGN KEY ("ModuleId") REFERENCES "Modules" ("Id") ON DELETE RESTRICT;
                  END IF;
              END IF;

              ALTER TABLE "MenuDefinitions" DROP COLUMN IF EXISTS "ServiceName";
              ALTER TABLE "MenuDefinitions" DROP COLUMN IF EXISTS "ServiceTranslationKey";
              ALTER TABLE "MenuDefinitions" DROP COLUMN IF EXISTS "TranslationKey";
              ALTER TABLE "MenuDefinitions" DROP COLUMN IF EXISTS "ServiceDisplayOrder";
            END $$;
            """,
            cancellationToken
        );
    }

    private static Guid DeterministicIdFromCodes(string a, string b)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes($"{a}|{b}");
        var hash = System.Security.Cryptography.MD5.HashData(bytes);
        return new Guid(hash);
    }

    private static IReadOnlyList<ModuleSeed> Modules { get; } =
    [
        new(ModuleGeneralId, ModuleCodes.General, MenuServiceNames.General, "dashboard", 0, true),
        new(ModuleStockId, ModuleCodes.Stock, MenuServiceNames.StockService, "warehouse", 10, false),
        new(ModuleIamId, ModuleCodes.Iam, MenuServiceNames.UserService, "security", 20, true),
        new(ModuleSystemId, ModuleCodes.System, MenuServiceNames.System, "system", 30, true),
    ];

    private static IReadOnlyList<(string ModuleCode, string Tr, string En)> ModuleNames { get; } =
    [
        (ModuleCodes.General, "Genel", "General"),
        (ModuleCodes.Stock, "Stok", "Stock"),
        (ModuleCodes.Iam, "Hesap & Güvenlik", "Account & Security"),
        (ModuleCodes.System, "Sistem", "System"),
    ];

    /// <summary>Idempotent seed — Code sabit, Id deterministik.</summary>
    internal static IReadOnlyList<MenuSeedRow> Catalog { get; } =
    [
        new(Guid.Parse("11111111-1111-1111-1111-111111110001"), "general.dashboard", ModuleCodes.General, "/", "dashboard", null, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110101"), "stock.units", ModuleCodes.Stock, "/units-of-measure", "units", "stock.read", 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110102"), "stock.categories", ModuleCodes.Stock, "/categories", "categories", "stock.read", 20),
        new(Guid.Parse("11111111-1111-1111-1111-111111110103"), "stock.products", ModuleCodes.Stock, "/products", "products", "stock.read", 30),
        new(Guid.Parse("11111111-1111-1111-1111-111111110104"), "stock.warehouses", ModuleCodes.Stock, "/warehouses", "warehouses", "stock.read", 40),
        new(Guid.Parse("11111111-1111-1111-1111-111111110105"), "stock.movements", ModuleCodes.Stock, "/stock-movements", "movements", "stock.read", 50),
        new(Guid.Parse("11111111-1111-1111-1111-111111110106"), "stock.balances", ModuleCodes.Stock, "/stock-balances", "balances", "stock.read", 60),
        new(Guid.Parse("11111111-1111-1111-1111-111111110201"), "iam.security", ModuleCodes.Iam, "/account/security", "security", null, 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110202"), "iam.users", ModuleCodes.Iam, "/admin/users", "users", "users.read", 20),
        new(Guid.Parse("11111111-1111-1111-1111-111111110203"), "iam.roles", ModuleCodes.Iam, "/admin/roles", "roles", "users.read", 30),
        new(Guid.Parse("11111111-1111-1111-1111-111111110301"), "system.overview", ModuleCodes.System, "/admin/system", "system", "system.read", 10),
        new(Guid.Parse("11111111-1111-1111-1111-111111110302"), "system.menus", ModuleCodes.System, "/system/menus", "menus", "system.manage", 20),
    ];

    private static IReadOnlyList<(string MenuCode, string Tr, string En)> MenuNames { get; } =
    [
        ("general.dashboard", "Genel Bakış", "Overview"),
        ("stock.units", "Ölçü Birimleri", "Units of Measure"),
        ("stock.categories", "Kategoriler", "Categories"),
        ("stock.products", "Ürünler", "Products"),
        ("stock.warehouses", "Depolar", "Warehouses"),
        ("stock.movements", "Stok Hareketleri", "Stock Movements"),
        ("stock.balances", "Stok Bakiyeleri", "Stock Balances"),
        ("iam.security", "Oturumlarım & Güvenlik", "Sessions & Security"),
        ("iam.users", "Kullanıcı Yönetimi", "User Management"),
        ("iam.roles", "Roller & İzinler", "Roles & Permissions"),
        ("system.overview", "Sistem", "System"),
        ("system.menus", "Menü Yönetimi", "Menu Management"),
    ];

    internal sealed record ModuleSeed(
        Guid Id,
        string Code,
        string ServiceName,
        string IconKey,
        int DisplayOrder,
        bool IsCore
    );

    internal sealed record MenuSeedRow(
        Guid Id,
        string Code,
        string ModuleCode,
        string RoutePath,
        string IconKey,
        string? RequiredPermission,
        int DisplayOrder
    );
}
