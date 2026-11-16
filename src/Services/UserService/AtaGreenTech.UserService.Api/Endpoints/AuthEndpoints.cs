using AtaGreenTech.UserService.Api.Contracts;
using AtaGreenTech.UserService.Api.Hosting;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.SetupMfa;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa;
using AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/auth</c>: oturum, MFA, e-posta doğrulama ve şifre yönetimi. Tenant host'tan çözülür.
/// </summary>
public sealed class AuthEndpoints : IEndpointModule
{
    public string Name => "auth";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.PolicyName);
        group.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        group.MapPost("/logout", LogoutAsync).AllowAnonymous();
        group.MapGet("/me", MeAsync).RequireAuthorization();
        group.MapPost("/verify-email", VerifyEmailAsync).AllowAnonymous();
        group.MapPost("/resend-verification", ResendVerificationAsync)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.PolicyName);
        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.PolicyName);
        group.MapPost("/reset-password", ResetPasswordAsync)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.PolicyName);
        group.MapPost("/change-password", ChangePasswordAsync).RequireAuthorization();
        group.MapPost("/accept-invite", AcceptInviteAsync)
            .AllowAnonymous()
            .RequireRateLimiting(AuthRateLimiting.PolicyName);

        group.MapPost("/mfa/setup", SetupMfaAsync).RequireAuthorization();
        group.MapPost("/mfa/verify", VerifyMfaAsync).RequireAuthorization();
        group.MapPost("/mfa/disable", DisableMfaAsync).RequireAuthorization();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new LoginCommand(
                request.Email,
                request.Password,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString(),
                request.TotpCode,
                request.RecoveryCode
            ),
            cancellationToken
        );

        return result.IsSuccess
            ? TypedResults.Ok(result.Response)
            : AuthError(result);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new RefreshTokenCommand(
                request.RefreshToken,
                httpContext.Connection.RemoteIpAddress?.ToString(),
                httpContext.Request.Headers.UserAgent.ToString()
            ),
            cancellationToken
        );

        return result.IsSuccess
            ? TypedResults.Ok(result.Response)
            : AuthError(result);
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
            ? AuthError(
                AuthErrorCodes.InvalidCredentials,
                "Geçerli bir oturum bulunamadı."
            )
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
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new ResetPasswordCommand(
                request.Token,
                request.NewPassword,
                httpContext.Connection.RemoteIpAddress?.ToString()
            ),
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
            return AuthError(AuthErrorCodes.InvalidCredentials, ex.Message);
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

    private static async Task<IResult> SetupMfaAsync(ISender sender, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new SetupMfaCommand(), cancellationToken);
            return result.Succeeded && result.Setup is not null
                ? TypedResults.Ok(result.Setup)
                : Results.Json(
                    new ErrorResponse("AUTH_BAD_REQUEST", result.Message ?? "MFA kurulumu başarısız."),
                    statusCode: StatusCodes.Status400BadRequest
                );
        }
        catch (UnauthorizedAccessException ex)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, ex.Message);
        }
    }

    private static async Task<IResult> VerifyMfaAsync(
        VerifyMfaRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(new VerifyMfaCommand(request.Code), cancellationToken);
            return ToMessageResult(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, ex.Message);
        }
    }

    private static async Task<IResult> DisableMfaAsync(
        DisableMfaRequest request,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(new DisableMfaCommand(request.Password), cancellationToken);
            return ToMessageResult(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, ex.Message);
        }
    }

    private static IResult ToMessageResult(Application.Features.Auth.MessageResult result) =>
        result.Succeeded
            ? TypedResults.Ok(new { message = result.Message })
            : Results.Json(
                new ErrorResponse("AUTH_BAD_REQUEST", result.Message),
                statusCode: StatusCodes.Status400BadRequest
            );

    private static IResult AuthError(AuthAttemptResult result) =>
        Results.Json(
            new ErrorResponse(
                result.ErrorCode ?? AuthErrorCodes.InvalidCredentials,
                result.ErrorMessage ?? "Kimlik doğrulama başarısız.",
                result.MfaRequired
            ),
            statusCode: result.StatusCode
        );

    private static IResult AuthError(
        string code,
        string message,
        int statusCode = StatusCodes.Status401Unauthorized,
        bool mfaRequired = false
    ) =>
        Results.Json(new ErrorResponse(code, message, mfaRequired), statusCode: statusCode);
}

/// <summary>Login istek gövdesi. MFA açıksa <see cref="TotpCode"/> veya <see cref="RecoveryCode"/> gerekir.</summary>
public sealed record LoginRequest(
    string Email,
    string Password,
    string? TotpCode = null,
    string? RecoveryCode = null
);

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

/// <summary>MFA kurulum doğrulama istek gövdesi.</summary>
public sealed record VerifyMfaRequest(string Code);

/// <summary>MFA kapatma istek gövdesi.</summary>
public sealed record DisableMfaRequest(string Password);
