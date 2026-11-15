using AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/auth</c>: kimlik doğrulama. Login anonimdir; tenant host'tan çözülür.
/// </summary>
public sealed class AuthEndpoints : IEndpointModule
{
    public string Name => "auth";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/login", LoginAsync).AllowAnonymous();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await sender.Send(
                new LoginCommand(request.Email, request.Password, httpContext.Connection.RemoteIpAddress?.ToString()),
                cancellationToken
            );
            return TypedResults.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Problem(
                title: "Unauthorized",
                detail: ex.Message,
                statusCode: StatusCodes.Status401Unauthorized
            );
        }
    }
}

/// <summary>Login istek gövdesi.</summary>
public sealed record LoginRequest(string Email, string Password);
