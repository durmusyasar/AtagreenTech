using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Users.Commands.CreateUser;
using AtaGreenTech.UserService.Application.Features.Users.Commands.DeleteUser;
using AtaGreenTech.UserService.Application.Features.Users.Commands.InviteUser;
using AtaGreenTech.UserService.Application.Features.Users.Commands.SetUserActiveStatus;
using AtaGreenTech.UserService.Application.Features.Users.Commands.UpdateUser;
using AtaGreenTech.UserService.Application.Features.Users.Queries.GetUserById;
using AtaGreenTech.UserService.Application.Features.Users.Queries.GetUsers;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/users</c>: tenant kullanıcı yönetimi (liste, CRUD, davet, aktiflik). Permission claim politikaları.
/// </summary>
public sealed class UserEndpoints : IEndpointModule
{
    public string Name => "users";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            string? searchTerm = null,
            bool? isActive = null
        ) => sender.Send(new GetUsersQuery(pageNumber, pageSize, searchTerm, isActive), cancellationToken))
            .RequireAuthorization(AuthorizationPolicies.UsersRead);

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(new GetUserByIdQuery(id), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.UsersRead);

        group.MapPost("", CreateAsync).RequireAuthorization(AuthorizationPolicies.UsersWrite);
        group.MapPost("/invite", InviteAsync).RequireAuthorization(AuthorizationPolicies.UsersWrite);
        group.MapPut("/{id:guid}", UpdateAsync).RequireAuthorization(AuthorizationPolicies.UsersWrite);
        group.MapPatch("/{id:guid}/activate", (Guid id, ISender sender, CancellationToken ct) =>
            SetActiveAsync(id, true, sender, ct)).RequireAuthorization(AuthorizationPolicies.UsersDelete);
        group.MapPatch("/{id:guid}/deactivate", (Guid id, ISender sender, CancellationToken ct) =>
            SetActiveAsync(id, false, sender, ct)).RequireAuthorization(AuthorizationPolicies.UsersDelete);
        group.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization(AuthorizationPolicies.UsersDelete);
    }

    private static async Task<IResult> CreateAsync(
        CreateUserRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new CreateUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Roles.ToArray(),
                request.IsActive
            ),
            cancellationToken
        );

        if (!result.Succeeded || result.User is null)
            return BadRequest(result.Message);

        return TypedResults.Created($"/api/v1/users/{result.User.Id}", result.User);
    }

    private static async Task<IResult> InviteAsync(
        InviteUserRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new InviteUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Role,
                httpContext.Connection.RemoteIpAddress?.ToString()
            ),
            cancellationToken
        );

        return result.Succeeded
            ? TypedResults.Ok(new { message = result.Message })
            : BadRequest(result.Message);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new UpdateUserCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Roles?.ToArray(),
                httpContext.Connection.RemoteIpAddress?.ToString()
            ),
            cancellationToken
        );

        return result.Succeeded && result.User is not null
            ? TypedResults.Ok(result.User)
            : BadRequest(result.Message);
    }

    private static async Task<IResult> SetActiveAsync(
        Guid id,
        bool isActive,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new SetUserActiveStatusCommand(id, isActive), cancellationToken);
        return ToNoContent(result);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new DeleteUserCommand(id), cancellationToken);
        return ToNoContent(result);
    }

    private static IResult ToNoContent(MessageResult result) =>
        result.Succeeded ? TypedResults.NoContent() : BadRequest(result.Message);

    private static IResult BadRequest(string detail) =>
        Results.Problem(
            title: "Bad Request",
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest
        );
}

/// <summary>Admin kullanıcı davet istek gövdesi.</summary>
public sealed record InviteUserRequest(string Email, string FirstName, string LastName, string Role);

/// <summary>Admin doğrudan kullanıcı oluşturma istek gövdesi.</summary>
public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IEnumerable<string> Roles,
    bool IsActive = true
);

/// <summary>Admin kullanıcı güncelleme istek gövdesi.</summary>
public sealed record UpdateUserRequest(
    string FirstName,
    string LastName,
    IEnumerable<string>? Roles
);
