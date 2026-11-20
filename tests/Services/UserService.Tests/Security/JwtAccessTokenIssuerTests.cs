using System.IdentityModel.Tokens.Jwt;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Infrastructure.Security;
using Microsoft.Extensions.Options;
using OmniSync.Core.Infrastructure.Authentication;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class JwtAccessTokenIssuerTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SessionId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private const string SecurityStamp = "AABBCCDDEEFF00112233445566778899";
    private const string RoleNameAdmin = "Admin";

    [Fact]
    public void Create_emits_contract_claims_and_user_service_issuer()
    {
        var issuer = CreateIssuer(accessTokenMinutes: 30);
        var result = issuer.Create(
            UserId,
            "admin@omnisync.com",
            TenantId,
            SessionId,
            SecurityStamp,
            [RoleNameAdmin],
            [Permissions.Users.Read, Permissions.Stock.Write]
        );

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);

        Assert.Equal(JwtContract.Issuer, jwt.Issuer);
        Assert.Contains(JwtContract.Audience, jwt.Audiences);
        Assert.Equal(UserId.ToString(), jwt.Subject);
        Assert.Equal("admin@omnisync.com", jwt.Claims.First(c => c.Type == JwtContract.Claims.Email).Value);
        Assert.Equal(TenantId.ToString(), jwt.Claims.First(c => c.Type == JwtContract.Claims.TenantId).Value);
        Assert.Equal(SessionId.ToString(), jwt.Claims.First(c => c.Type == JwtContract.Claims.SessionId).Value);
        Assert.Equal(SecurityStamp, jwt.Claims.First(c => c.Type == JwtContract.Claims.SecurityStamp).Value);
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
        Assert.True(result.ExpiresAt <= DateTime.UtcNow.AddMinutes(31));
    }

    [Fact]
    public void Create_dedupes_roles_and_permissions_case_insensitively()
    {
        var issuer = CreateIssuer(accessTokenMinutes: 15);
        var result = issuer.Create(
            UserId,
            "admin@omnisync.com",
            TenantId,
            SessionId,
            SecurityStamp,
            ["Admin", "admin", "  ", "Manager"],
            [Permissions.Users.Read, "USERS.READ", Permissions.Stock.Read, ""]
        );

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        var roles = jwt.Claims.Where(c => c.Type == JwtContract.Claims.Role).Select(c => c.Value).ToArray();
        var permissions = jwt.Claims.Where(c => c.Type == JwtContract.Claims.Permission).Select(c => c.Value).ToArray();

        Assert.Equal(2, roles.Length);
        Assert.Contains("Admin", roles);
        Assert.Contains("Manager", roles);
        Assert.Equal(2, permissions.Length);
        Assert.Contains(Permissions.Users.Read, permissions);
        Assert.Contains(Permissions.Stock.Read, permissions);
    }

    [Fact]
    public void Create_uses_default_lifetime_when_configured_minutes_invalid()
    {
        var issuer = CreateIssuer(accessTokenMinutes: 0);
        var before = DateTime.UtcNow;
        var result = issuer.Create(
            UserId,
            "admin@omnisync.com",
            TenantId,
            SessionId,
            SecurityStamp,
            [RoleNameAdmin],
            []
        );

        var expected = before.AddMinutes(JwtContract.DefaultAccessTokenMinutes);
        Assert.True(result.ExpiresAt >= expected.AddSeconds(-2));
        Assert.True(result.ExpiresAt <= expected.AddSeconds(5));
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public void Create_rejects_missing_required_identity(
        bool emptyUser,
        bool emptyTenant,
        bool emptySession,
        bool blankStamp
    )
    {
        var issuer = CreateIssuer(accessTokenMinutes: 30);
        Assert.ThrowsAny<ArgumentException>(() =>
            issuer.Create(
                emptyUser ? Guid.Empty : UserId,
                "admin@omnisync.com",
                emptyTenant ? Guid.Empty : TenantId,
                emptySession ? Guid.Empty : SessionId,
                blankStamp ? "  " : SecurityStamp,
                [RoleNameAdmin],
                []
            )
        );
    }

    [Fact]
    public void Create_rejects_blank_email()
    {
        var issuer = CreateIssuer(accessTokenMinutes: 30);
        Assert.ThrowsAny<ArgumentException>(() =>
            issuer.Create(UserId, "  ", TenantId, SessionId, SecurityStamp, [RoleNameAdmin], [])
        );
    }

    private static JwtAccessTokenIssuer CreateIssuer(int accessTokenMinutes) =>
        new(
            Options.Create(
                new JwtOptions
                {
                    Issuer = JwtContract.Issuer,
                    Audience = JwtContract.Audience,
                    SigningKey = "development-only-Z6dH/YzPeA4FFIvzDwCVyQiIlmHeHhRSQ44FBefGRbhs",
                }
            ),
            Options.Create(new AuthOptions { AccessTokenMinutes = accessTokenMinutes })
        );
}
