using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// JWT doğrulamasını ve tenant bağlamını uçtan uca denemek için kontrol noktası: <c>/api/v1/security-test</c>.
/// </summary>
public sealed class SecurityTestEndpoints : IEndpointModule
{
    public string Name => "security-test";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/protected", (ICurrentUser user, ICurrentTenant tenant) => TypedResults.Ok(new
            {
                Message = "AtaGreen Tech Stock Service yetkilendirme başarılı!",
                user.UserId,
                user.Email,
                user.Roles,
                Tenant = tenant.IsAvailable ? tenant.Identifier : null,
                Timestamp = DateTime.UtcNow,
            }))
            .RequireAuthorization();
    }
}
