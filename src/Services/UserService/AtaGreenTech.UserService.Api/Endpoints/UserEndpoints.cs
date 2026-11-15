using AtaGreenTech.UserService.Application.Features.Users.Commands.InviteUser;
using AtaGreenTech.UserService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/users</c>: kullanıcı yönetimi (davet). Admin rolü gerektirir.
/// </summary>
public sealed class UserEndpoints : IEndpointModule
{
    public string Name => "users";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/invite", InviteAsync)
            .RequireAuthorization(new AuthorizeAttribute { Roles = Role.Names.Admin });
    }

    private static async Task<IResult> InviteAsync(
        InviteUserRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new InviteUserCommand(request.Email, request.FirstName, request.LastName, request.Role),
            cancellationToken
        );

        return result.Succeeded
            ? TypedResults.Ok(new { message = result.Message })
            : Results.Problem(
                title: "Bad Request",
                detail: result.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
    }
}

/// <summary>Admin kullanıcı davet istek gövdesi.</summary>
public sealed record InviteUserRequest(string Email, string FirstName, string LastName, string Role);
