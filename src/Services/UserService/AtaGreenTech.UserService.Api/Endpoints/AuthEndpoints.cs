using AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;
using AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/auth</c>: login, refresh, logout, me ve e-posta doğrulama. Tenant host'tan çözülür.
/// </summary>
public sealed class AuthEndpoints : IEndpointModule
{
    public string Name => "auth";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/login", LoginAsync).AllowAnonymous();
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).AllowAnonymous();
        group.MapGet("/me", MeAsync).RequireAuthorization();
        group.MapPost("/verify-email", VerifyEmailAsync).AllowAnonymous();
        group.MapPost("/resend-verification", ResendVerificationAsync).AllowAnonymous();
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
            return Unauthorized(ex.Message);
        }
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken, httpContext.Connection.RemoteIpAddress?.ToString()),
            cancellationToken
        );

        return result.IsSuccess
            ? TypedResults.Ok(result.Response)
            : Unauthorized(result.ErrorMessage!);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return TypedResults.Ok(new { message = "Başarıyla çıkış yapıldı." });
    }

    private static async Task<IResult> MeAsync(ISender sender, CancellationToken cancellationToken)
    {
        var me = await sender.Send(new GetMeQuery(), cancellationToken);
        return me is null
            ? Unauthorized("Geçerli bir oturum bulunamadı.")
            : TypedResults.Ok(me);
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new VerifyEmailCommand(request.Token), cancellationToken);
        return result.Succeeded
            ? TypedResults.Ok(new { message = result.Message })
            : Results.Problem(
                title: "Bad Request",
                detail: result.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
    }

    private static async Task<IResult> ResendVerificationAsync(
        ResendVerificationRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new ResendVerificationCommand(request.Email), cancellationToken);
        return TypedResults.Ok(new { message = result.Message });
    }

    private static IResult Unauthorized(string detail) =>
        Results.Problem(
            title: "Unauthorized",
            detail: detail,
            statusCode: StatusCodes.Status401Unauthorized
        );
}

/// <summary>Login istek gövdesi.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Refresh istek gövdesi.</summary>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>Logout istek gövdesi.</summary>
public sealed record LogoutRequest(string RefreshToken);

/// <summary>E-posta doğrulama istek gövdesi.</summary>
public sealed record VerifyEmailRequest(string Token);

/// <summary>Doğrulama e-postasını yeniden gönderme istek gövdesi.</summary>
public sealed record ResendVerificationRequest(string Email);
