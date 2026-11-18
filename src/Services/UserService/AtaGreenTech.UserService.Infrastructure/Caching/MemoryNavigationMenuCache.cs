using System.Collections.Concurrent;
using AtaGreenTech.UserService.Application.Features.Navigation.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AtaGreenTech.UserService.Infrastructure.Caching;

internal sealed class MemoryNavigationMenuCache(IMemoryCache memoryCache) : INavigationMenuCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _tenantKeys = new();

    public bool TryGet(string cacheKey, out NavigationMenusResponse? response)
    {
        if (memoryCache.TryGetValue(cacheKey, out NavigationMenusResponse? value) && value is not null)
        {
            response = value;
            return true;
        }

        response = null;
        return false;
    }

    public void Set(string cacheKey, NavigationMenusResponse response)
    {
        memoryCache.Set(cacheKey, response, Ttl);

        if (TryParseTenantId(cacheKey, out var tenantId))
        {
            var keys = _tenantKeys.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal));
            keys[cacheKey] = 0;
        }
    }

    public void InvalidateTenant(Guid tenantId)
    {
        if (!_tenantKeys.TryRemove(tenantId, out var keys))
            return;

        foreach (var key in keys.Keys)
            memoryCache.Remove(key);
    }

    private static bool TryParseTenantId(string cacheKey, out Guid tenantId)
    {
        // nav:menus:{guid}:{hash}
        tenantId = default;
        var parts = cacheKey.Split(':', 4);
        return parts.Length >= 3 && Guid.TryParse(parts[2], out tenantId);
    }
}
