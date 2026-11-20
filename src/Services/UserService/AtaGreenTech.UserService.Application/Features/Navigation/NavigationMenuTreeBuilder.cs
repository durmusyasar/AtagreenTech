using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Navigation.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;

namespace AtaGreenTech.UserService.Application.Features.Navigation;

/// <summary>
/// Yetki filtresi + ParentId hiyerarşisi. Birim testlerinde Master/DbContext olmadan kullanılır.
/// </summary>
public static class NavigationMenuTreeBuilder
{
    public static NavigationMenusResponse Build(
        IReadOnlyList<NavigationMenuRow> rows,
        IReadOnlyList<string> userPermissions,
        string? preferredLanguage,
        string? defaultLanguage
    )
    {
        var allowed = rows
            .Where(r =>
                string.IsNullOrWhiteSpace(r.RequiredPermission)
                || PermissionMatcher.Satisfies(userPermissions, r.RequiredPermission)
            )
            .ToList();

        var byParent = allowed
            .GroupBy(r => r.ParentId)
            .ToDictionary(g => g.Key ?? Guid.Empty, g => g.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code).ToList());

        var roots = byParent.GetValueOrDefault(Guid.Empty) ?? [];

        var groups = roots
            .GroupBy(r => (r.ModuleCode, r.ServiceName, r.ModuleDisplayOrder))
            .OrderBy(g => g.Key.ModuleDisplayOrder)
            .ThenBy(g => g.Key.ServiceName, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var sample = g.First();
                var moduleTranslations = sample.ModuleTranslations;
                return new NavigationMenuGroupDto(
                    g.Key.ModuleCode,
                    g.Key.ServiceName,
                    NavigationTitleResolver.Resolve(
                        moduleTranslations,
                        preferredLanguage,
                        defaultLanguage,
                        g.Key.ModuleCode
                    ),
                    moduleTranslations,
                    g.Select(r => ToItem(r, byParent, preferredLanguage, defaultLanguage, [])).ToArray()
                );
            })
            .Where(g => g.Items.Count > 0)
            .ToArray();

        // Yetki filtresinden bağımsız: tenant'ta abone (görünür) modül kodları — #67 ModuleGuard.
        var subscribedModuleCodes = rows
            .Select(r => r.ModuleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new NavigationMenusResponse(groups, subscribedModuleCodes);
    }

    private static NavigationMenuItemDto ToItem(
        NavigationMenuRow row,
        IReadOnlyDictionary<Guid, List<NavigationMenuRow>> byParent,
        string? preferredLanguage,
        string? defaultLanguage,
        HashSet<Guid> ancestors
    )
    {
        // Döngüsel ParentId zincirinde stack overflow'u engelle.
        if (!ancestors.Add(row.Id))
        {
            return new NavigationMenuItemDto(
                row.Code,
                row.RoutePath,
                row.IconKey,
                NavigationTitleResolver.Resolve(
                    row.Translations,
                    preferredLanguage,
                    defaultLanguage,
                    row.Code
                ),
                row.Translations,
                null,
                row.RequiredPermission
            );
        }

        try
        {
            var children = byParent.GetValueOrDefault(row.Id);
            IReadOnlyList<NavigationMenuItemDto>? childDtos = null;
            if (children is { Count: > 0 })
            {
                childDtos = children
                    .Select(c => ToItem(c, byParent, preferredLanguage, defaultLanguage, ancestors))
                    .ToArray();
            }

            return new NavigationMenuItemDto(
                row.Code,
                row.RoutePath,
                row.IconKey,
                NavigationTitleResolver.Resolve(
                    row.Translations,
                    preferredLanguage,
                    defaultLanguage,
                    row.Code
                ),
                row.Translations,
                childDtos,
                row.RequiredPermission
            );
        }
        finally
        {
            ancestors.Remove(row.Id);
        }
    }
}
