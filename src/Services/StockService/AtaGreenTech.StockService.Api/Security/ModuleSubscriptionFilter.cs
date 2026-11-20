using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.StockService.Application.Interfaces;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.StockService.Api.Security;

/// <summary>
/// Endpoint grubunda tenant'ın ilgili modüle abone olduğunu doğrular (#67).
/// Abonelik yoksa 403 + <see cref="ModuleSubscriptionErrorCodes.NotSubscribed"/>.
/// </summary>
public sealed class ModuleSubscriptionFilter(string moduleCode) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var httpContext = context.HttpContext;
        var currentTenant = httpContext.RequestServices.GetRequiredService<ICurrentTenant>();
        var checker = httpContext.RequestServices.GetRequiredService<IModuleSubscriptionChecker>();

        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            return Results.Unauthorized();

        var isSubscribed = await checker.IsModuleActiveAsync(
            currentTenant.Id.Value,
            moduleCode,
            httpContext.RequestAborted
        );

        if (!isSubscribed)
        {
            return Results.Json(
                new
                {
                    errorCode = ModuleSubscriptionErrorCodes.NotSubscribed,
                    moduleCode,
                    detail =
                        $"Tenant does not have an active subscription for module '{moduleCode}'.",
                },
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        return await next(context);
    }
}

public static class ModuleEndpointExtensions
{
    public static RouteGroupBuilder RequireModule(this RouteGroupBuilder group, string moduleCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleCode);
        return group.AddEndpointFilter(new ModuleSubscriptionFilter(moduleCode));
    }
}
