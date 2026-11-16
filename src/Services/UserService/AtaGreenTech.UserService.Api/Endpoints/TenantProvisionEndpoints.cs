using AtaGreenTech.UserService.Api.Contracts;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using Microsoft.Extensions.Options;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// Dahili provisioning: <c>POST /api/v1/internal/tenants/seed</c>.
/// Yalnızca <c>X-Internal-Provision-Key</c> ile; nginx üzerinden public expose edilmemelidir.
/// </summary>
public sealed class TenantProvisionEndpoints : IEndpointModule
{
    public string Name => "internal";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/tenants/seed", SeedAsync).AllowAnonymous();
    }

    private static async Task<IResult> SeedAsync(
        TenantProvisionSeedHttpRequest body,
        HttpRequest httpRequest,
        IOptions<InternalProvisioningOptions> provisioningOptions,
        ITenantExecutionContext tenants,
        CancellationToken cancellationToken
    )
    {
        var expectedKey = provisioningOptions.Value.ApiKey;
        var providedKey = httpRequest.Headers["X-Internal-Provision-Key"].ToString();

        if (string.IsNullOrWhiteSpace(expectedKey)
            || !string.Equals(expectedKey, providedKey, StringComparison.Ordinal))
        {
            return TypedResults.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(body.TenantId))
        {
            return TypedResults.BadRequest(
                new ErrorResponse("TENANT_SEED_INVALID_INPUT", "TenantId (subdomain) zorunludur.")
            );
        }

        var subdomain = body.TenantId.Trim().ToLowerInvariant();
        var seedRequest = new TenantIdentitySeedRequest(
            body.AdminEmail,
            body.AdminPassword,
            body.AdminUsername,
            body.AdminFirstName,
            body.AdminLastName
        );

        try
        {
            TenantIdentitySeedResult? result = null;
            await tenants.RunInTenantScopeAsync(
                subdomain,
                async (sp, ct) =>
                {
                    var seeder = sp.GetRequiredService<ITenantIdentitySeeder>();
                    result = await seeder.EnsureAsync(seedRequest, TenantIdentitySeedMode.UpsertAdmin, ct);
                },
                cancellationToken
            );

            return TypedResults.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(new ErrorResponse("TENANT_SEED_INVALID_INPUT", ex.Message));
        }
        catch (Exception ex)
        {
            return TypedResults.Problem(
                title: "Tenant identity seed failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }
}

/// <summary>HTTP seed gövdesi. <c>tenantId</c> = subdomain.</summary>
public sealed record TenantProvisionSeedHttpRequest(
    string TenantId,
    string? AdminEmail = null,
    string? AdminPassword = null,
    string? AdminUsername = null,
    string? AdminFirstName = null,
    string? AdminLastName = null
);
