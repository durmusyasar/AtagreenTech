using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Roles.Queries.GetPermissions;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/permissions</c>: granüler izin kataloğu (RBAC yönetim UI).
/// </summary>
public sealed class PermissionEndpoints : IEndpointModule
{
    public string Name => "permissions";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", async (ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(new GetPermissionsQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.UsersRead);
    }
}
