using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Yetki politikası reddinde 403 + <see cref="AccessTokenErrorCodes.PermissionDenied"/> döner (#70).
/// </summary>
public sealed class PermissionDeniedAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult
    )
    {
        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                new
                {
                    errorCode = AccessTokenErrorCodes.PermissionDenied,
                    detail = "You do not have permission to perform this action.",
                }
            );
            return;
        }

        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
