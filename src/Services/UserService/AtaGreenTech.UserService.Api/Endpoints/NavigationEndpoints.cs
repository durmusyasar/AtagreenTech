using AtaGreenTech.UserService.Application.Features.Navigation;
using AtaGreenTech.UserService.Application.Features.Navigation.Queries.GetNavigationMenus;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/navigation</c>: tenant + izin filtreli dinamik menü ağacı.
/// </summary>
public sealed class NavigationEndpoints : IEndpointModule
{
    public string Name => "navigation";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/menus", GetMenusAsync)
            .RequireAuthorization()
            .WithTags("Navigation");
    }

    private static async Task<IResult> GetMenusAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var acceptLanguage = request.Headers.AcceptLanguage.FirstOrDefault();
        var preferred = string.IsNullOrWhiteSpace(acceptLanguage)
            ? null
            : NavigationTitleResolver.Normalize(acceptLanguage);

        return TypedResults.Ok(
            await sender.Send(new GetNavigationMenusQuery(preferred), cancellationToken)
        );
    }
}
