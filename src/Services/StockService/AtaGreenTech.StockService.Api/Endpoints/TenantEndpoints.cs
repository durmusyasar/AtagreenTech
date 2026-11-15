using AtaGreenTech.StockService.Application.Interfaces;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/tenant</c>: aktif tenant'ın marka ayarları (logo, renk, favicon). Login ekranı için anonimdir.
/// </summary>
public sealed class TenantEndpoints : IEndpointModule
{
    public string Name => "tenant";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/config", GetConfigAsync).AllowAnonymous();
    }

    private static async Task<IResult> GetConfigAsync(
        ICurrentTenant tenant,
        ITenantBrandingReader brandings,
        CancellationToken cancellationToken
    )
    {
        if (!tenant.IsAvailable || tenant.Id is null)
            return TypedResults.BadRequest(new { detail = "Tenant could not be resolved from the host." });

        var branding = await brandings.GetByTenantIdAsync(
            tenant.Id.Value,
            tenant.Identifier ?? string.Empty,
            tenant.Name ?? tenant.Identifier ?? "Tenant",
            cancellationToken
        );

        return branding is null ? TypedResults.NotFound() : TypedResults.Ok(branding);
    }
}
