using AtaGreenTech.SharedKernel.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Endpoint seviyesinde izin zorunluluğu; 403 gövdesinde <c>PERMISSION_DENIED</c> üretir (#70 / #71).
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
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IErrorLocalizer>();
            return Results.Json(
                new
                {
                    errorCode = AccessTokenErrorCodes.PermissionDenied,
                    permission,
                    detail = localizer.Localize(ErrorCodes.PermissionDenied),
                },
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        return await next(context);
    }
}
