using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Navigation.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Navigation.Queries.GetNavigationMenus;

internal sealed class GetNavigationMenusQueryHandler(
    INavigationMenuCatalog catalog,
    INavigationMenuCache cache,
    IUserDbContext db,
    ICurrentUser currentUser,
    ICurrentTenant currentTenant
) : IQueryHandler<GetNavigationMenusQuery, NavigationMenusResponse>
{
    public async Task<NavigationMenusResponse> Handle(
        GetNavigationMenusQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            throw new ForbiddenAccessException("Authentication required.");

        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            throw new ForbiddenAccessException("Tenant could not be resolved.");

        var tenantId = currentTenant.Id.Value;
        var permissions = await UserPermissionResolver.ResolveAsync(db, userId, cancellationToken);
        var cacheKey = BuildCacheKey(tenantId, currentUser.Roles, permissions);

        if (cache.TryGet(cacheKey, out var cached) && cached is not null)
            return cached;

        var rows = await catalog.GetActiveMenuRowsAsync(tenantId, cancellationToken);
        var response = NavigationMenuTreeBuilder.Build(rows, permissions);
        cache.Set(cacheKey, response);
        return response;
    }

    internal static string BuildCacheKey(
        Guid tenantId,
        IEnumerable<string>? roles,
        IReadOnlyList<string> permissions
    )
    {
        var rolesPart = string.Join(',', (roles ?? []).OrderBy(r => r, StringComparer.OrdinalIgnoreCase));
        var permsPart = string.Join(',', permissions.OrderBy(p => p, StringComparer.OrdinalIgnoreCase));
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{rolesPart}|{permsPart}")))
            .ToLowerInvariant()[..16];
        return $"nav:menus:{tenantId:D}:{hash}";
    }
}
