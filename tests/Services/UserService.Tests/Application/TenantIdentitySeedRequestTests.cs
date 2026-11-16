using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class TenantIdentitySeedRequestTests
{
    [Fact]
    public void PasswordRules_accept_default_bootstrap_password()
    {
        var (ok, message) = PasswordRules.Validate("Admin123!");
        Assert.True(ok, message);
    }

    [Fact]
    public void Seed_modes_distinguish_login_bootstrap_from_upsert()
    {
        Assert.NotEqual(TenantIdentitySeedMode.BootstrapIfEmpty, TenantIdentitySeedMode.UpsertAdmin);
    }
}
