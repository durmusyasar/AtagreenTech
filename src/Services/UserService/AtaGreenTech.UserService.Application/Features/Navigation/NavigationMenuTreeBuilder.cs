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
        IReadOnlyList<string> userPermissions
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
            .GroupBy(r => (r.ServiceName, r.ServiceTranslationKey, r.ServiceDisplayOrder))
            .OrderBy(g => g.Key.ServiceDisplayOrder)
            .ThenBy(g => g.Key.ServiceName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new NavigationMenuGroupDto(
                g.Key.ServiceName,
                g.Key.ServiceTranslationKey,
                g.Select(r => ToItem(r, byParent)).ToArray()
            ))
            .Where(g => g.Items.Count > 0)
            .ToArray();

        return new NavigationMenusResponse(groups);
    }

    private static NavigationMenuItemDto ToItem(
        NavigationMenuRow row,
        IReadOnlyDictionary<Guid, List<NavigationMenuRow>> byParent
    )
    {
        var children = byParent.GetValueOrDefault(row.Id);
        IReadOnlyList<NavigationMenuItemDto>? childDtos = null;
        if (children is { Count: > 0 })
        {
            childDtos = children.Select(c => ToItem(c, byParent)).ToArray();
        }

        return new NavigationMenuItemDto(
            row.Code,
            row.RoutePath,
            row.IconKey,
            row.TranslationKey,
            childDtos
        );
    }
}
