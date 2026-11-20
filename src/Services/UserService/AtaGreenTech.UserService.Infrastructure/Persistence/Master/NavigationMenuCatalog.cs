using System.Text.Json;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Master;

internal sealed class NavigationMenuCatalog(MasterDbContext master) : INavigationMenuCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<NavigationCatalogSnapshot> GetActiveMenusAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default
    )
    {
        // Tenant'ta abonelik yoksa varsayılanları ekle (yeni tenant / migrate sırası).
        await master.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "TenantServiceSubscriptions" ("Id", "TenantId", "ServiceName", "IsEnabled", "SubscribedAtUtc")
            SELECT gen_random_uuid(), {tenantId}, s."ServiceName", s."IsEnabled", now()
            FROM (VALUES
                ('General', TRUE),
                ('UserService', TRUE),
                ('StockService', TRUE),
                ('System', TRUE)
            ) AS s("ServiceName", "IsEnabled")
            WHERE NOT EXISTS (
                SELECT 1 FROM "TenantServiceSubscriptions" x
                WHERE x."TenantId" = {tenantId} AND x."ServiceName" = s."ServiceName"
            );
            """,
            cancellationToken
        );

        var defaultLanguage = await master.Database
            .SqlQueryRaw<LanguageCodeDto>(
                """
                SELECT "Code"
                FROM "Languages"
                WHERE "IsDefault" = TRUE AND "IsActive" = TRUE
                ORDER BY "Code"
                LIMIT 1
                """
            )
            .Select(r => r.Code)
            .FirstOrDefaultAsync(cancellationToken);

        // Önce DTO'yu materialize et; JSON parse EF çevirisine girmez.
        var dtoRows = await master.Database
            .SqlQueryRaw<NavigationMenuRowDto>(
                """
                SELECT m."Id",
                       m."Code",
                       mod."Code" AS "ModuleCode",
                       mod."ServiceName",
                       m."ParentId",
                       m."RoutePath",
                       m."IconKey",
                       m."RequiredPermission",
                       m."DisplayOrder",
                       mod."DisplayOrder" AS "ModuleDisplayOrder",
                       COALESCE((
                           SELECT jsonb_object_agg(l."Code", ml."Name")
                           FROM "ModuleLanguages" ml
                           INNER JOIN "Languages" l ON l."Id" = ml."LanguageId" AND l."IsActive" = TRUE
                           WHERE ml."ModuleId" = mod."Id"
                       ), '{}'::jsonb)::text AS "ModuleTranslationsJson",
                       COALESCE((
                           SELECT jsonb_object_agg(l."Code", menul."Name")
                           FROM "MenuLanguages" menul
                           INNER JOIN "Languages" l ON l."Id" = menul."LanguageId" AND l."IsActive" = TRUE
                           WHERE menul."MenuId" = m."Id"
                       ), '{}'::jsonb)::text AS "TranslationsJson"
                FROM "MenuDefinitions" m
                INNER JOIN "Modules" mod ON mod."Id" = m."ModuleId" AND mod."IsActive" = TRUE
                INNER JOIN "TenantServiceSubscriptions" s
                    ON s."ServiceName" = mod."ServiceName"
                   AND s."TenantId" = {0}
                   AND s."IsEnabled" = TRUE
                WHERE m."IsActive" = TRUE
                ORDER BY mod."DisplayOrder", m."DisplayOrder", m."Code"
                """,
                tenantId
            )
            .ToListAsync(cancellationToken);

        var rows = dtoRows
            .Select(r => new NavigationMenuRow(
                r.Id,
                r.Code,
                r.ModuleCode,
                r.ServiceName,
                r.ParentId,
                r.RoutePath,
                r.IconKey,
                r.RequiredPermission,
                r.DisplayOrder,
                r.ModuleDisplayOrder,
                ParseTranslations(r.ModuleTranslationsJson),
                ParseTranslations(r.TranslationsJson)
            ))
            .ToList();

        return new NavigationCatalogSnapshot(rows, defaultLanguage);
    }

    private static IReadOnlyDictionary<string, string> ParseTranslations(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json is "{}" or "null")
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions);
        return map is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class LanguageCodeDto
    {
        public string Code { get; set; } = null!;
    }

    private sealed class NavigationMenuRowDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string ModuleCode { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public string RoutePath { get; set; } = null!;
        public string IconKey { get; set; } = null!;
        public string? RequiredPermission { get; set; }
        public int DisplayOrder { get; set; }
        public int ModuleDisplayOrder { get; set; }
        public string ModuleTranslationsJson { get; set; } = "{}";
        public string TranslationsJson { get; set; } = "{}";
    }
}
