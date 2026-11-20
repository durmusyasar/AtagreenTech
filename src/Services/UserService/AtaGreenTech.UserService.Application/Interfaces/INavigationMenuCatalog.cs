namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Master DB menü kataloğu + tenant servis abonelikleri.</summary>
public interface INavigationMenuCatalog
{
    /// <summary>
    /// Tenant için etkin servislerdeki aktif menü satırlarını ve varsayılan dil kodunu döner.
    /// </summary>
    Task<NavigationCatalogSnapshot> GetActiveMenusAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default
    );
}

public sealed record NavigationCatalogSnapshot(
    IReadOnlyList<NavigationMenuRow> Rows,
    string? DefaultLanguageCode
);

/// <summary>Master'dan okunan düz menü satırı + dil map.</summary>
public sealed record NavigationMenuRow(
    Guid Id,
    string Code,
    string ModuleCode,
    string ServiceName,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    int ModuleDisplayOrder,
    IReadOnlyDictionary<string, string> ModuleTranslations,
    IReadOnlyDictionary<string, string> Translations
);
