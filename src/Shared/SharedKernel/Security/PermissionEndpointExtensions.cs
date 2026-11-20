using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Endpoint seviyesinde izin zorunluluğu; 403 gövdesinde <c>PERMISSION_DENIED</c> üretir (#70).
/// Mevcut <c>RequireAuthorization(perm:*)</c> politikaları da aynı hata kodunu
/// <see cref="PermissionDeniedAuthorizationResultHandler"/> ile döner.
/// </summary>
public static class PermissionEndpointExtensions
{
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        string permission
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return builder
            .RequireAuthorization()
            .AddEndpointFilter(new RequirePermissionFilter(permission));
    }

    public static RouteGroupBuilder RequirePermission(
        this RouteGroupBuilder group,
        string permission
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return group.AddEndpointFilter(new RequirePermissionFilter(permission));
    }
}

internal sealed class RequirePermissionFilter(string permission) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated is not true)
            return Results.Unauthorized();

        if (!PermissionMatcher.HasPermission(user, permission))
        {
            return Results.Json(
                new
                {
                    errorCode = AccessTokenErrorCodes.PermissionDenied,
                    permission,
                    detail = $"Missing required permission '{permission}'.",
                },
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        return await next(context);
    }
}
