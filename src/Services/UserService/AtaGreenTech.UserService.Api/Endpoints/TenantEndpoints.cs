using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Tenants.Commands.CreateTenant;
using AtaGreenTech.UserService.Application.Features.Tenants.Commands.UpdateTenantModules;
using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using AtaGreenTech.UserService.Application.Features.Tenants.Queries.GetTenantModules;
using AtaGreenTech.UserService.Application.Features.Tenants.Queries.ListTenants;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/tenants</c>: platform tenant listesi, oluşturma ve modül aboneliği (master DB).
/// </summary>
public sealed class TenantEndpoints : IEndpointModule
{
    public string Name => "tenants";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", ListAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemTenantsManage)
            .WithTags("Tenants");

        group.MapPost("", CreateAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemTenantsManage)
            .WithTags("Tenants");

        group.MapGet("/{id:guid}/modules", GetModulesAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemTenantsManage)
            .WithTags("Tenants");

        group.MapPut("/{id:guid}/modules", UpdateModulesAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemTenantsManage)
            .WithTags("Tenants");
    }

    private static async Task<IResult> ListAsync(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        string? search = null
    )
    {
        var result = await sender.Send(new ListTenantsQuery(page, pageSize, search), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CreateAsync(
        CreateTenantRequest body,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new CreateTenantCommand(
                body.Name,
                body.Slug,
                body.Modules,
                body.AdminEmail,
                body.AdminFirstName,
                body.AdminLastName,
                body.AdminPassword
            ),
            cancellationToken
        );
        return TypedResults.Created($"/api/v1/tenants/{result.Id}", result);
    }

    private static async Task<IResult> GetModulesAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken,
        string? lang = null
    )
    {
        var result = await sender.Send(new GetTenantModulesQuery(id, lang), cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> UpdateModulesAsync(
        Guid id,
        UpdateTenantModulesHttpRequest body,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new UpdateTenantModulesCommand(id, body.Modules ?? []),
            cancellationToken
        );
        return TypedResults.NoContent();
    }
}

public sealed record UpdateTenantModulesHttpRequest(IReadOnlyList<TenantModuleItemRequest>? Modules);
