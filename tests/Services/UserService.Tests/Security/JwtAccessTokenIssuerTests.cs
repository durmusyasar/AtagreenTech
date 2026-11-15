using System.IdentityModel.Tokens.Jwt;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Infrastructure.Security;
using Microsoft.Extensions.Options;
using OmniSync.Core.Infrastructure.Authentication;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class JwtAccessTokenIssuerTests
{
    [Fact]
    public void Create_emits_contract_claims_and_user_service_issuer()
    {
        var issuer = new JwtAccessTokenIssuer(
            Options.Create(new JwtOptions
            {
                Issuer = JwtContract.Issuer,
                Audience = JwtContract.Audience,
                SigningKey = "development-only-Z6dH/YzPeA4FFIvzDwCVyQiIlmHeHhRSQ44FBefGRbhs",
            }),
            Options.Create(new AuthOptions { AccessTokenMinutes = 30 })
        );

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var tenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var result = issuer.Create(
            userId,
            "admin@omnisync.com",
            tenantId,
            [RoleNameAdmin],
            [Permissions.Users.Read, Permissions.Stock.Write]
        );

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Equal(JwtContract.Issuer, jwt.Issuer);
        Assert.Contains(JwtContract.Audience, jwt.Audiences);
        Assert.Equal(userId.ToString(), jwt.Subject);
        Assert.Equal("admin@omnisync.com", jwt.Claims.First(c => c.Type == JwtContract.Claims.Email).Value);
        Assert.Equal(tenantId.ToString(), jwt.Claims.First(c => c.Type == JwtContract.Claims.TenantId).Value);
        Assert.Equal(RoleNameAdmin, jwt.Claims.First(c => c.Type == JwtContract.Claims.Role).Value);
        Assert.Contains(
            Permissions.Users.Read,
            jwt.Claims.Where(c => c.Type == JwtContract.Claims.Permission).Select(c => c.Value)
        );
        Assert.Contains(
            Permissions.Stock.Write,
            jwt.Claims.Where(c => c.Type == JwtContract.Claims.Permission).Select(c => c.Value)
        );
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    private const string RoleNameAdmin = "Admin";
}

