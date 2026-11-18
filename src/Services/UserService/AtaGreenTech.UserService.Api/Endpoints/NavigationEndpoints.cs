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
        group.MapGet("/menus", async (ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetNavigationMenusQuery(), cancellationToken)))
            .RequireAuthorization()
            .WithTags("Navigation");
    }
}
