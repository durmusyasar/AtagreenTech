using System.Text.Json;
using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Master;

internal sealed class SystemNavigationAdmin(MasterDbContext master) : ISystemNavigationAdmin
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<IReadOnlyList<SystemLanguageDto>> GetLanguagesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default
    )
    {
        var sql = activeOnly
            ? """
              SELECT "Id", "Code", "Name", "IsDefault", "IsActive"
              FROM "Languages"
              WHERE "IsActive" = TRUE
              ORDER BY "IsDefault" DESC, "Code"
              """
            : """
              SELECT "Id", "Code", "Name", "IsDefault", "IsActive"
              FROM "Languages"
              ORDER BY "IsDefault" DESC, "Code"
              """;

        return await master.Database
            .SqlQueryRaw<LanguageRow>(sql)
            .Select(r => new SystemLanguageDto(r.Id, r.Code, r.Name, r.IsDefault, r.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemModuleDto>> GetModulesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var rows = await master.Database
            .SqlQueryRaw<ModuleRow>(
                """
                SELECT mod."Id", mod."Code", mod."ServiceName", mod."IconKey", mod."DisplayOrder",
                       mod."IsCore", mod."IsActive",
                       COALESCE((
                           SELECT jsonb_object_agg(l."Code", ml."Name")
                           FROM "ModuleLanguages" ml
                           INNER JOIN "Languages" l ON l."Id" = ml."LanguageId"
                           WHERE ml."ModuleId" = mod."Id"
                       ), '{}'::jsonb)::text AS "TranslationsJson"
                FROM "Modules" mod
                ORDER BY mod."DisplayOrder", mod."Code"
                """
            )
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new SystemModuleDto(
                r.Id,
                r.Code,
                r.ServiceName,
                r.IconKey,
                r.DisplayOrder,
                r.IsCore,
                r.IsActive,
                ParseTranslations(r.TranslationsJson)
            ))
            .ToArray();
    }

    public async Task<IReadOnlyList<SystemMenuListItemDto>> GetMenusAsync(
        CancellationToken cancellationToken = default
    )
    {
        var rows = await master.Database
            .SqlQueryRaw<MenuListRow>(
                """
                SELECT m."Id", m."Code", m."ModuleId", mod."Code" AS "ModuleCode", m."ParentId",
                       m."RoutePath", m."IconKey", m."RequiredPermission", m."DisplayOrder", m."IsActive",
                       COALESCE((
                           SELECT jsonb_object_agg(l."Code", ml."Name")
                           FROM "MenuLanguages" ml
                           INNER JOIN "Languages" l ON l."Id" = ml."LanguageId"
                           WHERE ml."MenuId" = m."Id"
                       ), '{}'::jsonb)::text AS "TranslationsJson"
                FROM "MenuDefinitions" m
                INNER JOIN "Modules" mod ON mod."Id" = m."ModuleId"
                ORDER BY mod."DisplayOrder", m."DisplayOrder", m."Code"
                """
            )
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new SystemMenuListItemDto(
                r.Id,
                r.Code,
                r.ModuleId,
                r.ModuleCode,
                r.ParentId,
                r.RoutePath,
                r.IconKey,
                r.RequiredPermission,
                r.DisplayOrder,
                r.IsActive,
                ParseTranslations(r.TranslationsJson)
            ))
            .ToArray();
    }

    public async Task<SystemMenuDetailDto?> GetMenuAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var row = await master.Database
            .SqlQueryRaw<MenuDetailRow>(
                """
                SELECT m."Id", m."Code", m."ModuleId", m."ParentId", m."RoutePath", m."IconKey",
                       m."RequiredPermission", m."DisplayOrder", m."IsActive",
                       COALESCE((
                           SELECT jsonb_object_agg(l."Code", ml."Name")
                           FROM "MenuLanguages" ml
                           INNER JOIN "Languages" l ON l."Id" = ml."LanguageId"
                           WHERE ml."MenuId" = m."Id"
                       ), '{}'::jsonb)::text AS "TranslationsJson"
                FROM "MenuDefinitions" m
                WHERE m."Id" = {0}
                """,
                id
            )
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new SystemMenuDetailDto(
                row.Id,
                row.Code,
                row.ModuleId,
                row.ParentId,
                row.RoutePath,
                row.IconKey,
                row.RequiredPermission,
                row.DisplayOrder,
                row.IsActive,
                ParseTranslations(row.TranslationsJson)
            );
    }

    public async Task<Guid> CreateMenuAsync(
        CreateSystemMenuRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureModuleExistsAsync(request.ModuleId, cancellationToken);
        if (request.ParentId is { } parentId)
            await EnsureMenuExistsAsync(parentId, cancellationToken);

        var code = NormalizeCode(request.Code);
        await EnsureCodeAvailableAsync(code, excludeId: null, cancellationToken);

        var languageMap = await ResolveLanguageIdsAsync(request.Translations, cancellationToken);
        var id = Guid.NewGuid();

        await using var tx = await master.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "MenuDefinitions" (
                    "Id", "Code", "ModuleId", "ParentId", "RoutePath", "IconKey",
                    "RequiredPermission", "DisplayOrder", "IsActive"
                ) VALUES (
                    {id}, {code}, {request.ModuleId}, {request.ParentId},
                    {NormalizeRoute(request.RoutePath)}, {NormalizeIcon(request.IconKey)},
                    {NormalizePermission(request.RequiredPermission)}, {request.DisplayOrder},
                    {request.IsActive}
                );
                """,
                cancellationToken
            );

            await UpsertMenuTranslationsAsync(id, languageMap, cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return id;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateMenuAsync(
        Guid id,
        UpdateSystemMenuRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureMenuExistsAsync(id, cancellationToken);
        await EnsureModuleExistsAsync(request.ModuleId, cancellationToken);
        if (request.ParentId is { } parentId)
        {
            if (parentId == id)
                throw new InvalidOperationException("A menu cannot be its own parent.");
            await EnsureMenuExistsAsync(parentId, cancellationToken);
            await EnsureNotDescendantAsync(id, parentId, cancellationToken);
        }

        var code = NormalizeCode(request.Code);
        await EnsureCodeAvailableAsync(code, excludeId: id, cancellationToken);
        var languageMap = await ResolveLanguageIdsAsync(request.Translations, cancellationToken);

        await using var tx = await master.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var affected = await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "MenuDefinitions" SET
                    "Code" = {code},
                    "ModuleId" = {request.ModuleId},
                    "ParentId" = {request.ParentId},
                    "RoutePath" = {NormalizeRoute(request.RoutePath)},
                    "IconKey" = {NormalizeIcon(request.IconKey)},
                    "RequiredPermission" = {NormalizePermission(request.RequiredPermission)},
                    "DisplayOrder" = {request.DisplayOrder},
                    "IsActive" = {request.IsActive}
                WHERE "Id" = {id};
                """,
                cancellationToken
            );

            if (affected == 0)
                throw new NotFoundException(nameof(MenuDefinition), id);

            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""DELETE FROM "MenuLanguages" WHERE "MenuId" = {id};""",
                cancellationToken
            );
            await UpsertMenuTranslationsAsync(id, languageMap, cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task PatchMenuAsync(
        Guid id,
        PatchSystemMenuRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureMenuExistsAsync(id, cancellationToken);

        if (request.IsActive is null && request.DisplayOrder is null)
            return;

        if (request.IsActive is { } isActive && request.DisplayOrder is { } displayOrder)
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "MenuDefinitions"
                SET "IsActive" = {isActive}, "DisplayOrder" = {displayOrder}
                WHERE "Id" = {id};
                """,
                cancellationToken
            );
            return;
        }

        if (request.IsActive is { } activeOnly)
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""UPDATE "MenuDefinitions" SET "IsActive" = {activeOnly} WHERE "Id" = {id};""",
                cancellationToken
            );
            return;
        }

        await master.Database.ExecuteSqlInterpolatedAsync(
            $"""UPDATE "MenuDefinitions" SET "DisplayOrder" = {request.DisplayOrder!.Value} WHERE "Id" = {id};""",
            cancellationToken
        );
    }

    public async Task UpdateModuleAsync(
        Guid id,
        UpdateSystemModuleRequest request,
        CancellationToken cancellationToken = default
    )
    {
        await EnsureModuleExistsAsync(id, cancellationToken);
        var languageMap = await ResolveLanguageIdsAsync(request.Translations, cancellationToken);

        await using var tx = await master.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                UPDATE "Modules" SET
                    "IconKey" = {NormalizeIcon(request.IconKey)},
                    "DisplayOrder" = {request.DisplayOrder},
                    "IsActive" = {request.IsActive}
                WHERE "Id" = {id};
                """,
                cancellationToken
            );

            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""DELETE FROM "ModuleLanguages" WHERE "ModuleId" = {id};""",
                cancellationToken
            );

            foreach (var (languageId, name) in languageMap)
            {
                var rowId = Guid.NewGuid();
                await master.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                    INSERT INTO "ModuleLanguages" ("Id", "ModuleId", "LanguageId", "Name")
                    VALUES ({rowId}, {id}, {languageId}, {name});
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

    private async Task UpsertMenuTranslationsAsync(
        Guid menuId,
        IReadOnlyList<(Guid LanguageId, string Name)> translations,
        CancellationToken cancellationToken
    )
    {
        foreach (var (languageId, name) in translations)
        {
            var rowId = Guid.NewGuid();
            await master.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "MenuLanguages" ("Id", "MenuId", "LanguageId", "Name")
                VALUES ({rowId}, {menuId}, {languageId}, {name});
                """,
                cancellationToken
            );
        }
    }

    private async Task<IReadOnlyList<(Guid LanguageId, string Name)>> ResolveLanguageIdsAsync(
        IReadOnlyList<SystemMenuTranslationInput> translations,
        CancellationToken cancellationToken
    )
    {
        if (translations.Count == 0)
            throw new InvalidOperationException("At least one translation is required.");

        var languages = await GetLanguagesAsync(activeOnly: true, cancellationToken);
        var byCode = languages.ToDictionary(l => l.Code, l => l, StringComparer.OrdinalIgnoreCase);
        var result = new List<(Guid, string)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in translations)
        {
            var code = item.LanguageCode.Trim().ToLowerInvariant();
            if (!seen.Add(code))
                throw new InvalidOperationException($"Duplicate translation for language '{code}'.");

            if (!byCode.TryGetValue(code, out var language))
                throw new InvalidOperationException($"Unknown or inactive language '{code}'.");

            var name = item.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"Translation name for '{code}' is required.");
            if (name.Length > MenuLanguage.NameMaxLength)
                throw new InvalidOperationException($"Translation name for '{code}' is too long.");

            result.Add((language.Id, name));
        }

        var defaultLang = languages.FirstOrDefault(l => l.IsDefault);
        if (defaultLang is not null && !seen.Contains(defaultLang.Code))
            throw new InvalidOperationException(
                $"Default language '{defaultLang.Code}' translation is required."
            );

        return result;
    }

    private async Task EnsureModuleExistsAsync(Guid moduleId, CancellationToken cancellationToken)
    {
        var exists = await master.Database
            .SqlQueryRaw<IdRow>(
                """SELECT "Id" FROM "Modules" WHERE "Id" = {0}""",
                moduleId
            )
            .AnyAsync(cancellationToken);

        if (!exists)
            throw new NotFoundException(nameof(Module), moduleId);
    }

    private async Task EnsureMenuExistsAsync(Guid menuId, CancellationToken cancellationToken)
    {
        var exists = await master.Database
            .SqlQueryRaw<IdRow>(
                """SELECT "Id" FROM "MenuDefinitions" WHERE "Id" = {0}""",
                menuId
            )
            .AnyAsync(cancellationToken);

        if (!exists)
            throw new NotFoundException(nameof(MenuDefinition), menuId);
    }

    /// <summary>
    /// <paramref name="candidateParentId"/> menünün kendi alt ağacında olamaz (döngü).
    /// </summary>
    private async Task EnsureNotDescendantAsync(
        Guid menuId,
        Guid candidateParentId,
        CancellationToken cancellationToken
    )
    {
        var links = await master.Database
            .SqlQueryRaw<ParentLinkRow>(
                """SELECT "Id", "ParentId" FROM "MenuDefinitions" """
            )
            .ToListAsync(cancellationToken);

        var parentById = links.ToDictionary(x => x.Id, x => x.ParentId);
        var cursor = candidateParentId;
        var guard = 0;
        while (guard++ < 256)
        {
            if (cursor == menuId)
                throw new InvalidOperationException("Cannot set a descendant as parent (cycle).");

            if (!parentById.TryGetValue(cursor, out var parent) || parent is null)
                return;

            cursor = parent.Value;
        }

        throw new InvalidOperationException("Menu hierarchy is too deep or cyclic.");
    }

    private async Task EnsureCodeAvailableAsync(
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken
    )
    {
        var existing = await master.Database
            .SqlQueryRaw<IdRow>(
                """SELECT "Id" FROM "MenuDefinitions" WHERE "Code" = {0}""",
                code
            )
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null && existing.Id != excludeId)
            throw new InvalidOperationException($"Menu code '{code}' already exists.");
    }

    private static string NormalizeCode(string code)
    {
        var value = code.Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Menu code is required.");
        if (value.Length > MenuDefinition.CodeMaxLength)
            throw new InvalidOperationException("Menu code is too long.");
        return value;
    }

    private static string NormalizeRoute(string route)
    {
        var value = string.IsNullOrWhiteSpace(route) ? "/" : route.Trim();
        if (value.Length > MenuDefinition.RoutePathMaxLength)
            throw new InvalidOperationException("Route path is too long.");
        return value;
    }

    private static string NormalizeIcon(string icon)
    {
        var value = string.IsNullOrWhiteSpace(icon) ? "menu" : icon.Trim();
        if (value.Length > MenuDefinition.IconKeyMaxLength)
            throw new InvalidOperationException("Icon key is too long.");
        return value;
    }

    private static string? NormalizePermission(string? permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return null;
        var value = permission.Trim();
        if (value.Length > MenuDefinition.RequiredPermissionMaxLength)
            throw new InvalidOperationException("Required permission is too long.");
        return value;
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

    private sealed class LanguageRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class ModuleRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string IconKey { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public bool IsCore { get; set; }
        public bool IsActive { get; set; }
        public string TranslationsJson { get; set; } = "{}";
    }

    private sealed class MenuListRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public Guid ModuleId { get; set; }
        public string ModuleCode { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public string RoutePath { get; set; } = null!;
        public string IconKey { get; set; } = null!;
        public string? RequiredPermission { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string TranslationsJson { get; set; } = "{}";
    }

    private sealed class MenuDetailRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public Guid ModuleId { get; set; }
        public Guid? ParentId { get; set; }
        public string RoutePath { get; set; } = null!;
        public string IconKey { get; set; } = null!;
        public string? RequiredPermission { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public string TranslationsJson { get; set; } = "{}";
    }

    private sealed class IdRow
    {
        public Guid Id { get; set; }
    }

    private sealed class ParentLinkRow
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
    }
}
