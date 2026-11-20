using AtaGreenTech.SharedKernel.Localization;
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
using AtaGreenTech.UserService.Application.Interfaces;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/auth</c>: oturum, MFA, SSO iskeleti, e-posta doğrulama ve şifre yönetimi. Tenant host'tan çözülür.
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

        // V1 OIDC SSO placeholder — feature-flag kapalı / henüz implemente değil → 501.
        group.MapGet("/sso/{provider}/login", SsoLoginAsync).AllowAnonymous();
        group.MapPost("/sso/{provider}/callback", SsoCallbackAsync).AllowAnonymous();
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        HttpContext httpContext,
        IErrorLocalizer localizer,
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

        return result.IsSuccess ? TypedResults.Ok(result.Response) : AuthError(result, localizer);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        ISender sender,
        HttpContext httpContext,
        IErrorLocalizer localizer,
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
            : AuthError(result, localizer);
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

    private static async Task<IResult> MeAsync(
        ISender sender,
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        var me = await sender.Send(new GetMeQuery(), cancellationToken);
        return me is null
            ? AuthError(AuthErrorCodes.InvalidCredentials, localizer)
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
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        if (!result.Succeeded)
        {
            return Results.Json(
                new ErrorResponse(
                    AuthErrorCodes.EmailNotFound,
                    localizer.Localize(AuthErrorCodes.EmailNotFound, result.Message)
                ),
                statusCode: StatusCodes.Status400BadRequest
            );
        }

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
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(
                new ChangePasswordCommand(request.CurrentPassword, request.NewPassword),
                cancellationToken
            );
            return ToMessageResult(result);
        }
        catch (UnauthorizedAccessException)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, localizer);
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

    private static async Task<IResult> SetupMfaAsync(
        ISender sender,
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
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
        catch (UnauthorizedAccessException)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, localizer);
        }
    }

    private static async Task<IResult> VerifyMfaAsync(
        VerifyMfaRequest request,
        ISender sender,
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(new VerifyMfaCommand(request.Code), cancellationToken);
            return ToMessageResult(result);
        }
        catch (UnauthorizedAccessException)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, localizer);
        }
    }

    private static async Task<IResult> DisableMfaAsync(
        DisableMfaRequest request,
        ISender sender,
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await sender.Send(new DisableMfaCommand(request.Password), cancellationToken);
            return ToMessageResult(result);
        }
        catch (UnauthorizedAccessException)
        {
            return AuthError(AuthErrorCodes.InvalidCredentials, localizer);
        }
    }

    private static async Task<IResult> SsoLoginAsync(
        string provider,
        string? redirectUri,
        IOidcAuthService oidcService,
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await oidcService.GetAuthorizationUrlAsync(
                provider,
                redirectUri ?? string.Empty,
                cancellationToken
            );
            return TypedResults.Ok(result);
        }
        catch (NotImplementedException)
        {
            return SsoNotImplemented(localizer);
        }
    }

    private static async Task<IResult> SsoCallbackAsync(
        string provider,
        OidcCallbackRequest request,
        IOidcAuthService oidcService,
        IErrorLocalizer localizer,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await oidcService.HandleCallbackAsync(provider, request, cancellationToken);
            return TypedResults.Ok(result);
        }
        catch (NotImplementedException)
        {
            return SsoNotImplemented(localizer);
        }
    }

    private static IResult SsoNotImplemented(IErrorLocalizer localizer) =>
        Results.Json(
            new ErrorResponse(
                AuthErrorCodes.SsoNotImplemented,
                localizer.Localize(AuthErrorCodes.SsoNotImplemented)
            ),
            statusCode: StatusCodes.Status501NotImplemented
        );

    private static IResult ToMessageResult(Application.Features.Auth.MessageResult result) =>
        result.Succeeded
            ? TypedResults.Ok(new { message = result.Message })
            : Results.Json(
                new ErrorResponse("AUTH_BAD_REQUEST", result.Message),
                statusCode: StatusCodes.Status400BadRequest
            );

    private static IResult AuthError(AuthAttemptResult result, IErrorLocalizer localizer)
    {
        var code = result.ErrorCode ?? AuthErrorCodes.InvalidCredentials;
        var args = result.ErrorArgs?.ToArray() ?? [];
        var message = localizer.Localize(code, result.ErrorMessage, args);
        return Results.Json(
            new ErrorResponse(code, message, result.MfaRequired),
            statusCode: result.StatusCode
        );
    }

    private static IResult AuthError(
        string code,
        IErrorLocalizer localizer,
        int statusCode = StatusCodes.Status401Unauthorized,
        bool mfaRequired = false,
        params object[] args
    ) =>
        Results.Json(
            new ErrorResponse(code, localizer.Localize(code, fallback: null, args), mfaRequired),
            statusCode: statusCode
        );
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
