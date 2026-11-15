using AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;
using AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/auth</c>: oturum, e-posta doğrulama ve şifre yönetimi. Tenant host'tan çözülür.
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
        group.MapPost("/forgot-password", ForgotPasswordAsync).AllowAnonymous();
        group.MapPost("/reset-password", ResetPasswordAsync).AllowAnonymous();
        group.MapPost("/change-password", ChangePasswordAsync).RequireAuthorization();
        group.MapPost("/accept-invite", AcceptInviteAsync).AllowAnonymous();
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
        return ToMessageResult(result);
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

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return TypedResults.Ok(new { message = result.Message });
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword),
            cancellationToken
        );
        return ToMessageResult(result);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(
                new ChangePasswordCommand(request.CurrentPassword, request.NewPassword),
                cancellationToken
            );
            return TypedResults.Ok(new { message = result.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    private static async Task<IResult> AcceptInviteAsync(
        AcceptInviteRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new AcceptInviteCommand(request.Token, request.Password, request.FirstName, request.LastName),
            cancellationToken
        );
        return ToMessageResult(result);
    }

    private static IResult ToMessageResult(Application.Features.Auth.MessageResult result) =>
        result.Succeeded
            ? TypedResults.Ok(new { message = result.Message })
            : Results.Problem(
                title: "Bad Request",
                detail: result.Message,
                statusCode: StatusCodes.Status400BadRequest
            );

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

/// <summary>Şifre unutma istek gövdesi.</summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Şifre sıfırlama istek gövdesi.</summary>
public sealed record ResetPasswordRequest(string Token, string NewPassword);

/// <summary>Oturumdayken şifre değiştirme istek gövdesi.</summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>Davet kabul istek gövdesi.</summary>
public sealed record AcceptInviteRequest(string Token, string Password, string FirstName, string LastName);
