using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Roles.Commands.UpdateRolePermissions;
using AtaGreenTech.UserService.Application.Features.Roles.Queries.GetRoles;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/roles</c>: rol listesi ve rol-izin eşlemesi (RBAC yönetim UI).
/// </summary>
public sealed class RoleEndpoints : IEndpointModule
{
    public string Name => "roles";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", async (ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(new GetRolesQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.UsersRead);

        group.MapPut("/{id:guid}/permissions", UpdatePermissionsAsync)
            .RequireAuthorization(AuthorizationPolicies.UsersWrite);
    }

    private static async Task<IResult> UpdatePermissionsAsync(
        Guid id,
        UpdateRolePermissionsRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new UpdateRolePermissionsCommand(
                id,
                request.Permissions?.ToArray() ?? [],
                httpContext.Connection.RemoteIpAddress?.ToString()
            ),
            cancellationToken
        );

        return result.Succeeded && result.Role is not null
            ? TypedResults.Ok(result.Role)
            : Results.Problem(
                title: "Bad Request",
                detail: result.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
    }
}

public sealed record UpdateRolePermissionsRequest(IEnumerable<string>? Permissions);
