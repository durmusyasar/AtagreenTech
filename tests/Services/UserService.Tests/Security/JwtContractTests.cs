using AtaGreenTech.SharedKernel.Security;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class JwtContractTests
{
    [Fact]
    public void Contract_matches_omnisync_short_claim_names_and_user_service_issuer()
    {
        Assert.Equal("AtaGreenTech.UserService", JwtContract.Issuer);
        Assert.Equal("AtaGreenTech.Apps", JwtContract.Audience);
        Assert.Equal("JwtSettings", JwtContract.SettingsSection);
        Assert.Equal(60, JwtContract.DefaultAccessTokenMinutes);
        Assert.Equal("sub", JwtContract.Claims.Subject);
        Assert.Equal("email", JwtContract.Claims.Email);
        Assert.Equal("role", JwtContract.Claims.Role);
        Assert.Equal("tenant_id", JwtContract.Claims.TenantId);
    }
}
