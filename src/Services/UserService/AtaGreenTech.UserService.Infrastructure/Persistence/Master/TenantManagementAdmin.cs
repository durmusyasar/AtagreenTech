using System.Text.Json;
using System.Text.RegularExpressions;
using AtaGreenTech.UserService.Application.Common;
using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Master;

internal sealed class TenantManagementAdmin(MasterDbContext master) : ITenantManagementAdmin
{
    private static readonly Regex SlugPattern = new(
        @"^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<TenantListResponse> ListTenantsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    )
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var offset = (page - 1) * pageSize;
        // Boş aramada % → tüm satırlar; null parametre SqlQueryRaw uyarısını önler.
        var term = string.IsNullOrWhiteSpace(search) ? "%" : $"%{search.Trim()}%";

        var total = await master.Database
            .SqlQueryRaw<CountRow>(
                """
                SELECT COUNT(*)::int AS "Value"
                FROM "Tenants" t
                WHERE NOT t."IsDeleted"
                  AND (t."Name" ILIKE {0} OR t."Subdomain" ILIKE {0})
                """,
                term
            )
            .Select(r => r.Value)
            .FirstAsync(cancellationToken);

        var items = await master.Database
            .SqlQueryRaw<TenantListRow>(
                """
                SELECT t."Id", t."Name", t."Subdomain" AS "Slug", t."IsActive",
                       t."CreatedAt" AS "CreatedAtUtc",
                       COALESCE((
                           SELECT COUNT(*)::int
                           FROM "TenantModuleSubscriptions" s
                           INNER JOIN "Modules" mod ON mod."Id" = s."ModuleId"
                           WHERE s."TenantId" = t."Id"
                             AND s."IsEnabled" = TRUE
                             AND (s."ExpiresAtUtc" IS NULL OR s."ExpiresAtUtc" > now())
                             AND mod."IsCore" = FALSE
                       ), 0) AS "EnabledModuleCount"
                FROM "Tenants" t
                WHERE NOT t."IsDeleted"
                  AND (t."Name" ILIKE {0} OR t."Subdomain" ILIKE {0})
                ORDER BY t."Name"
                OFFSET {1} LIMIT {2}
                """,
                term,
                offset,
                pageSize
            )
            .Select(r => new TenantListItemDto(
                r.Id,
                r.Name,
                r.Slug,
                r.IsActive,
                r.EnabledModuleCount,
                r.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return new TenantListResponse(items, total, page, pageSize);
    }

    public async Task<CreateTenantResult> CreateTenantAsync(
        CreateTenantRequest request,
        Guid? actorUserId,
        CancellationToken cancellationToken = default
    )
    {
        var name = request.Name.Trim();
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Tenant name is required.");
        if (!SlugPattern.IsMatch(slug) || slug is "www")
            throw new InvalidOperationException("Invalid tenant slug.");

        var exists = await master.Database
            .SqlQueryRaw<IdRow>(
                """
                SELECT "Id" FROM "Tenants"
                WHERE "Subdomain" = {0} AND NOT "IsDeleted"
                """,
                slug
            )
            .AnyAsync(cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Tenant slug '{slug}' already exists.");

        var templateCs = await master.Database
            .SqlQueryRaw<ConnectionStringRow>(
                """
                SELECT "ConnectionString" AS "Value"
                FROM "Tenants"
                WHERE NOT "IsDeleted" AND "ConnectionString" IS NOT NULL AND "ConnectionString" <> ''
                ORDER BY "CreatedAt"
                LIMIT 1
                """
            )
            .Select(r => r.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(templateCs))
            throw new InvalidOperationException("No template tenant connection string found.");

        var database = $"atagreen_stock_{slug.Replace('-', '_')}";
        var connectionString = Regex.Replace(
            templateCs,
            @"Database=[^;]+",
            $"Database={database}",
            RegexOptions.IgnoreCase
        );

        var id = Guid.NewGuid();
        await using var tx = await master.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "Tenants" ("Id", "Name", "Subdomain", "ConnectionString", "IsActive", "CreatedAt", "IsDeleted")
                VALUES ({id}, {name}, {slug}, {connectionString}, TRUE, now(), FALSE);
                """,
                cancellationToken
            );

            var enabledByModule = (request.Modules ?? [])
                .ToDictionary(m => m.ModuleId, m => (m.IsEnabled, m.ExpiresAtUtc));

            var modules = await master.Database
                .SqlQueryRaw<ModuleFlagRow>(
                    """SELECT "Id", "IsCore" FROM "Modules" WHERE "IsActive" = TRUE"""
                )
                .ToListAsync(cancellationToken);

            foreach (var mod in modules)
            {
                var enabled = mod.IsCore || (enabledByModule.TryGetValue(mod.Id, out var cfg) && cfg.IsEnabled);
                // İstek yoksa geriye dönük: tüm modüller açık.
                if (request.Modules is null || request.Modules.Count == 0)
                    enabled = true;
                if (mod.IsCore)
                    enabled = true;

                DateTime? expires = mod.IsCore
                    ? null
                    : (enabledByModule.TryGetValue(mod.Id, out var e) ? e.ExpiresAtUtc : null);

                var rowId = Guid.NewGuid();
                await master.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                    INSERT INTO "TenantModuleSubscriptions" (
                        "Id", "TenantId", "ModuleId", "IsEnabled", "ExpiresAtUtc", "UpdatedAtUtc", "UpdatedByUserId"
                    ) VALUES (
                        {rowId}, {id}, {mod.Id}, {enabled}, {expires}, now(), {actorUserId}
                    );
                    """,
                    cancellationToken
                );
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        return new CreateTenantResult(
            id,
            name,
            slug,
            IdentitySeeded: false,
            Message: "Tenant master kaydı ve abonelikler oluşturuldu. Tenant DB migration + identity seed (add-tenant / internal seed) ayrıca çalıştırılmalıdır."
        );
    }

    public async Task<IReadOnlyList<TenantModuleItemDto>> GetTenantModulesAsync(
        Guid tenantId,
        string? preferredLanguage,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureTenantExistsAsync(tenantId, cancellationToken);

        var lang = string.IsNullOrWhiteSpace(preferredLanguage) ? "tr" : preferredLanguage.Trim().ToLowerInvariant();

        var rows = await master.Database
            .SqlQueryRaw<TenantModuleRow>(
                """
                SELECT mod."Id" AS "ModuleId",
                       mod."Code",
                       mod."IsCore",
                       COALESCE(s."IsEnabled", mod."IsCore") AS "IsEnabled",
                       s."ExpiresAtUtc",
                       COALESCE((
                           SELECT jsonb_object_agg(l."Code", ml."Name")
                           FROM "ModuleLanguages" ml
                           INNER JOIN "Languages" l ON l."Id" = ml."LanguageId"
                           WHERE ml."ModuleId" = mod."Id"
                       ), '{{}}'::jsonb)::text AS "TranslationsJson"
                FROM "Modules" mod
                LEFT JOIN "TenantModuleSubscriptions" s
                    ON s."ModuleId" = mod."Id" AND s."TenantId" = {0}
                WHERE mod."IsActive" = TRUE
                ORDER BY mod."DisplayOrder", mod."Code"
                """,
                tenantId
            )
            .ToListAsync(cancellationToken);

        return rows
            .Select(r =>
            {
                var translations = ParseTranslations(r.TranslationsJson);
                var name = translations.TryGetValue(lang, out var localized) && !string.IsNullOrWhiteSpace(localized)
                    ? localized
                    : StringHelpers.FirstNonEmpty(translations.Values) ?? r.Code;
                return new TenantModuleItemDto(
                    r.ModuleId,
                    r.Code,
                    name,
                    translations,
                    r.IsCore,
                    r.IsCore || r.IsEnabled,
                    r.IsCore ? null : r.ExpiresAtUtc
                );
            })
            .ToArray();
    }

    public async Task UpdateTenantModulesAsync(
        Guid tenantId,
        IReadOnlyList<TenantModuleItemRequest> modules,
        Guid? actorUserId,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureTenantExistsAsync(tenantId, cancellationToken);

        var catalog = await master.Database
            .SqlQueryRaw<ModuleFlagRow>(
                """SELECT "Id", "IsCore" FROM "Modules" WHERE "IsActive" = TRUE"""
            )
            .ToListAsync(cancellationToken);
        var byId = catalog.ToDictionary(m => m.Id);

        foreach (var item in modules)
        {
            if (!byId.TryGetValue(item.ModuleId, out var mod))
                throw new NotFoundException("Module", item.ModuleId);

            if (mod.IsCore && !item.IsEnabled)
                throw new InvalidOperationException("Core modules cannot be disabled.");
        }

        await using var tx = await master.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var item in modules)
            {
                var mod = byId[item.ModuleId];
                var enabled = mod.IsCore || item.IsEnabled;
                DateTime? expires = mod.IsCore ? null : item.ExpiresAtUtc;

                await master.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                    INSERT INTO "TenantModuleSubscriptions" (
                        "Id", "TenantId", "ModuleId", "IsEnabled", "ExpiresAtUtc", "UpdatedAtUtc", "UpdatedByUserId"
                    ) VALUES (
                        {Guid.NewGuid()}, {tenantId}, {item.ModuleId}, {enabled}, {expires}, now(), {actorUserId}
                    )
                    ON CONFLICT ("TenantId", "ModuleId") DO UPDATE SET
                        "IsEnabled" = EXCLUDED."IsEnabled",
                        "ExpiresAtUtc" = EXCLUDED."ExpiresAtUtc",
                        "UpdatedAtUtc" = EXCLUDED."UpdatedAtUtc",
                        "UpdatedByUserId" = EXCLUDED."UpdatedByUserId";
                    """,
                    cancellationToken
                );
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task EnsureTenantExistsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var exists = await master.Database
            .SqlQueryRaw<IdRow>(
                """SELECT "Id" FROM "Tenants" WHERE "Id" = {0} AND NOT "IsDeleted" """,
                tenantId
            )
            .AnyAsync(cancellationToken);

        if (!exists)
            throw new NotFoundException("Tenant", tenantId);
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

    private sealed class CountRow
    {
        public int Value { get; set; }
    }

    private sealed class TenantListRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public bool IsActive { get; set; }
        public int EnabledModuleCount { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
    }

    private sealed class ConnectionStringRow
    {
        public string Value { get; set; } = null!;
    }

    private sealed class IdRow
    {
        public Guid Id { get; set; }
    }

    private sealed class ModuleFlagRow
    {
        public Guid Id { get; set; }
        public bool IsCore { get; set; }
    }

    private sealed class TenantModuleRow
    {
        public Guid ModuleId { get; set; }
        public string Code { get; set; } = null!;
        public bool IsCore { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string TranslationsJson { get; set; } = "{}";
    }
}
