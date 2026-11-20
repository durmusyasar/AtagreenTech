using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Api.Contracts;
using AtaGreenTech.UserService.Api.Contracts.Auth;
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
/// <c>/api/v1/auth</c>: oturum, MFA, SSO iskeleti, e-posta doğrulama ve şifre yönetimi.
/// Tenant host’tan çözülür; yanıt metinleri <see cref="IErrorLocalizer"/> (#71).
/// </summary>
public sealed class AuthEndpoints : IEndpointModule
{
    /// <inheritdoc />
    public string Name => "auth";

    /// <summary>Auth uçlarını <c>/api/v1/auth</c> grubuna kaydeder (AllowAnonymous / RequireAuthorization).</summary>
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

        // V1 OIDC SSO iskeleti — her zaman SSO_NOT_IMPLEMENTED → 501 (#71).
        group.MapGet("/sso/{provider}/login", SsoLoginAsync).AllowAnonymous();
        group.MapPost("/sso/{provider}/callback", SsoCallbackAsync).AllowAnonymous();
    }

    /// <summary>Email/şifre girişi; MFA açıksa TOTP veya kurtarma kodu. AllowAnonymous + rate limit.</summary>
    private static async Task<IResult> LoginAsync(LoginRequest request, ISender sender, HttpContext httpContext,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginCommand(request.Email, request.Password, ClientIp(httpContext),
                httpContext.Request.Headers.UserAgent.ToString(), request.TotpCode, request.RecoveryCode),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Response)
            : AuthError(result, localizer);
    }

    /// <summary>Refresh token ile access/refresh yeniler. AllowAnonymous.</summary>
    private static async Task<IResult> RefreshAsync(RefreshTokenRequest request, ISender sender,
        HttpContext httpContext, IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken, ClientIp(httpContext),
                httpContext.Request.Headers.UserAgent.ToString()),
            cancellationToken);

        return result.IsSuccess ? TypedResults.Ok(result.Response) : AuthError(result, localizer);
    }

    /// <summary>Refresh ailesi + access oturumunu iptal eder. AllowAnonymous, idempotent.</summary>
    private static async Task<IResult> LogoutAsync(LogoutRequest request, ISender sender, IErrorLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>JWT oturumundaki kullanıcının profil / rol / izin özeti. RequireAuthorization.</summary>
    private static async Task<IResult> MeAsync(ISender sender, IErrorLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMeQuery(), cancellationToken);
        return result.Succeeded
            ? TypedResults.Ok(result.Me)
            : AuthError(result.ErrorCode ?? AuthErrorCodes.InvalidCredentials, localizer, result.StatusCode);
    }

    /// <summary>E-posta doğrulama jetonunu işler. AllowAnonymous.</summary>
    private static async Task<IResult> VerifyEmailAsync(VerifyEmailRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyEmailCommand(request.Token), cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>Doğrulama e-postasını yeniden gönderir (enumeration-safe). AllowAnonymous + rate limit.</summary>
    private static async Task<IResult> ResendVerificationAsync(ResendVerificationRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResendVerificationCommand(request.Email), cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>Şifre sıfırlama e-postası başlatır (enumeration-safe). AllowAnonymous + rate limit.</summary>
    private static async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>Reset jetonu ile yeni parola yazar; oturumları iptal eder. AllowAnonymous + rate limit.</summary>
    private static async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request, ISender sender,
        IErrorLocalizer localizer, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword, ClientIp(httpContext)),
                cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>Oturumdayken parola değiştirir; tüm oturumları iptal eder. RequireAuthorization.</summary>
    private static async Task<IResult> ChangePasswordAsync(ChangePasswordRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangePasswordCommand(request.CurrentPassword, request.NewPassword),
            cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>Davet jetonu ile hesap oluşturur. AllowAnonymous + rate limit.</summary>
    private static async Task<IResult> AcceptInviteAsync(AcceptInviteRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result =
            await sender.Send(
                new AcceptInviteCommand(request.Token, request.Password, request.FirstName, request.LastName),
                cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>MFA enrollment: secret + QR URI + kurtarma kodları. RequireAuthorization.</summary>
    private static async Task<IResult> SetupMfaAsync(ISender sender, IErrorLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetupMfaCommand(), cancellationToken);
        if (result is { Succeeded: true, Setup: not null })
            return TypedResults.Ok(result.Setup);

        return BadRequestError(result.ErrorCode ?? ErrorCodes.AuthBadRequest, localizer, result.ErrorArgs);
    }

    /// <summary>Enrollment TOTP doğrular ve MFA’yı açar. RequireAuthorization.</summary>
    private static async Task<IResult> VerifyMfaAsync(VerifyMfaRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyMfaCommand(request.Code), cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>Parola ile MFA’yı kapatır. RequireAuthorization.</summary>
    private static async Task<IResult> DisableMfaAsync(DisableMfaRequest request, ISender sender,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DisableMfaCommand(request.Password), cancellationToken);
        return ToMessageResult(result, localizer);
    }

    /// <summary>OIDC authorization URL (V1: <c>SSO_NOT_IMPLEMENTED</c> → 501). AllowAnonymous.</summary>
    private static async Task<IResult> SsoLoginAsync(string provider, string? redirectUri, IOidcAuthService oidcService,
        IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await oidcService.GetAuthorizationUrlAsync(provider, redirectUri ?? string.Empty, cancellationToken);
            return TypedResults.Ok(result);
        }
        catch (NotImplementedException ex) when (ex.Message == ErrorCodes.SsoNotImplemented)
        {
            return SsoNotImplemented(localizer);
        }
    }

    /// <summary>OIDC callback (V1: <c>SSO_NOT_IMPLEMENTED</c> → 501). AllowAnonymous.</summary>
    private static async Task<IResult> SsoCallbackAsync(string provider, OidcCallbackRequest request,
        IOidcAuthService oidcService, IErrorLocalizer localizer, CancellationToken cancellationToken)
    {
        try
        {
            var result = await oidcService.HandleCallbackAsync(provider, request, cancellationToken);
            return TypedResults.Ok(result);
        }
        catch (NotImplementedException ex) when (ex.Message == ErrorCodes.SsoNotImplemented)
        {
            return SsoNotImplemented(localizer);
        }
    }

    /// <summary>V1 SSO iskeleti — 501 + lokalize <c>SSO_NOT_IMPLEMENTED</c>.</summary>
    private static IResult SsoNotImplemented(IErrorLocalizer localizer) =>
        Results.Json(
            new ErrorResponse(AuthErrorCodes.SsoNotImplemented, localizer.Localize(AuthErrorCodes.SsoNotImplemented)),
            statusCode: StatusCodes.Status501NotImplemented);

    /// <summary>
    /// <see cref="MessageResult"/> → HTTP. Başarıda <c>OkCode</c> resource’tan;
    /// hatada <see cref="ErrorCodes"/> + Args (#71).
    /// </summary>
    private static IResult ToMessageResult(MessageResult result, IErrorLocalizer localizer)
    {
        if (!result.Succeeded)
            return BadRequestError(result.ErrorCode ?? ErrorCodes.AuthBadRequest, localizer, result.ErrorArgs);

        var successCode = result.ErrorCode ?? result.Message;
        return TypedResults.Ok(new { message = localizer.Localize(successCode) });
    }

    /// <summary>400 + lokalize <see cref="ErrorResponse"/>.</summary>
    private static IResult BadRequestError(string code, IErrorLocalizer localizer,
        IReadOnlyList<object>? errorArgs = null)
    {
        var args = errorArgs?.ToArray() ?? [];
        return Results.Json(new ErrorResponse(code, localizer.Localize(code, fallback: null, args)),
            statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary><see cref="AuthAttemptResult"/> hata gövdesi (status + opsiyonel MFA bayrağı).</summary>
    private static IResult AuthError(AuthAttemptResult result, IErrorLocalizer localizer)
    {
        var code = result.ErrorCode ?? AuthErrorCodes.InvalidCredentials;
        var args = result.ErrorArgs?.ToArray() ?? [];
        return Results.Json(
            new ErrorResponse(code, localizer.Localize(code, fallback: null, args), result.MfaRequired),
            statusCode: result.StatusCode);
    }

    /// <summary>Kod + status ile auth hata gövdesi (örn. <c>/me</c>).</summary>
    private static IResult AuthError(string code, IErrorLocalizer localizer,
        int statusCode = StatusCodes.Status401Unauthorized, bool mfaRequired = false, params object[] args) =>
        Results.Json(new ErrorResponse(code, localizer.Localize(code, fallback: null, args), mfaRequired),
            statusCode: statusCode);

    /// <summary>İstemci IP (audit / refresh metadata; tenant kimliği değildir).</summary>
    private static string? ClientIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString();
}
