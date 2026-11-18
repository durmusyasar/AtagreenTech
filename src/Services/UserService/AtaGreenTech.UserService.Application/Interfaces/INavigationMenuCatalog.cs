namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Master DB menü kataloğu + tenant servis abonelikleri.</summary>
public interface INavigationMenuCatalog
{
    /// <summary>
    /// Tenant için etkin servislerdeki aktif menü satırlarını döner (yetki filtresi uygulanmamış).
    /// </summary>
    Task<IReadOnlyList<NavigationMenuRow>> GetActiveMenuRowsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default
    );
}

/// <summary>Master'dan okunan düz menü satırı.</summary>
public sealed record NavigationMenuRow(
    Guid Id,
    string Code,
    string ServiceName,
    string ServiceTranslationKey,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string TranslationKey,
    string? RequiredPermission,
    int DisplayOrder,
    int ServiceDisplayOrder
);
