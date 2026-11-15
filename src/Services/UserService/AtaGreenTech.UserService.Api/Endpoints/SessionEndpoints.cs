using AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeOtherSessions;
using AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeSession;
using AtaGreenTech.UserService.Application.Features.Sessions.Queries.GetSessions;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/sessions</c>: kullanıcının kendi aktif oturumlarını listeler / iptal eder.
/// </summary>
public sealed class SessionEndpoints : IEndpointModule
{
    public const string RefreshTokenHeaderName = "X-Refresh-Token";

    public string Name => "sessions";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", ListAsync).RequireAuthorization();
        group.MapDelete("/{id:guid}", RevokeAsync).RequireAuthorization();
        group.MapPost("/revoke-others", RevokeOthersAsync).RequireAuthorization();
    }

    private static async Task<IResult> ListAsync(
        ISender sender,
        HttpRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var current = request.Headers[RefreshTokenHeaderName].ToString();
            var sessions = await sender.Send(
                new GetSessionsQuery(string.IsNullOrWhiteSpace(current) ? null : current),
                cancellationToken
            );
            return TypedResults.Ok(sessions);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    private static async Task<IResult> RevokeAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(new RevokeSessionCommand(id), cancellationToken);
            return result.Succeeded ? TypedResults.NoContent() : BadRequest(result.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    private static async Task<IResult> RevokeOthersAsync(
        ISender sender,
        HttpRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var current = request.Headers[RefreshTokenHeaderName].ToString();
            if (string.IsNullOrWhiteSpace(current))
                return BadRequest("X-Refresh-Token header is required.");

            var result = await sender.Send(new RevokeOtherSessionsCommand(current), cancellationToken);
            return result.Succeeded
                ? TypedResults.Ok(new { message = result.Message })
                : BadRequest(result.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    private static IResult Unauthorized(string detail) =>
        Results.Problem(
            title: "Unauthorized",
            detail: detail,
            statusCode: StatusCodes.Status401Unauthorized
        );

    private static IResult BadRequest(string detail) =>
        Results.Problem(
            title: "Bad Request",
            detail: detail,
            statusCode: StatusCodes.Status400BadRequest
        );
}
