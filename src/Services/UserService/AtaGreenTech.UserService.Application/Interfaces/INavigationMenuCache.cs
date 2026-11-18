using AtaGreenTech.UserService.Application.Features.Navigation.DTOs;

namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Tenant + kullanıcı izin özeti bazlı navigasyon önbelleği.</summary>
public interface INavigationMenuCache
{
    bool TryGet(string cacheKey, out NavigationMenusResponse? response);

    void Set(string cacheKey, NavigationMenusResponse response);

    /// <summary>Tenant'a ait tüm <c>nav:menus:{tenantId}:*</c> girdilerini siler.</summary>
    void InvalidateTenant(Guid tenantId);
}
