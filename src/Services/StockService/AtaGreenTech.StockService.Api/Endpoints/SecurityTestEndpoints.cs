using System.Security.Claims;
using AtaGreenTech.SharedKernel.Security;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// UserService JWT doğrulaması ve stock permission politikalarını uçtan uca denemek için
/// kontrol noktası: <c>/api/v1/security-test</c>.
/// </summary>
public sealed class SecurityTestEndpoints : IEndpointModule
{
    public string Name => "security-test";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/protected", (
            ClaimsPrincipal principal,
            ICurrentUser user,
            ICurrentTenant tenant
        ) =>
            {
                var permissions = principal.FindAll(JwtContract.Claims.Permission)
                    .Select(c => c.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return TypedResults.Ok(new
                {
                    Message = "AtaGreen Tech Stock Service yetkilendirme başarılı!",
                    user.UserId,
                    user.Email,
                    user.Roles,
                    Permissions = permissions,
                    CanRead = PermissionMatcher.HasPermission(principal, Permissions.Stock.Read),
                    CanWrite = PermissionMatcher.HasPermission(principal, Permissions.Stock.Write),
                    CanDelete = PermissionMatcher.HasPermission(principal, Permissions.Stock.Delete),
                    Tenant = tenant.IsAvailable ? tenant.Identifier : null,
                    Timestamp = DateTime.UtcNow,
                });
            })
            .RequireAuthorization(AuthorizationPolicyRegistration.StockReadPolicy);

        group.MapGet("/write-check", () =>
            TypedResults.Ok(new { Message = "stock.write policy OK", Timestamp = DateTime.UtcNow }))
            .RequireAuthorization(AuthorizationPolicyRegistration.StockWritePolicy);

        group.MapGet("/delete-check", () =>
            TypedResults.Ok(new { Message = "stock.delete policy OK", Timestamp = DateTime.UtcNow }))
            .RequireAuthorization(AuthorizationPolicyRegistration.StockDeletePolicy);
    }
}
