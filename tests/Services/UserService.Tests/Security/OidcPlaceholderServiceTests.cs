using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class OidcPlaceholderServiceTests
{
    [Fact]
    public async Task GetAuthorizationUrlAsync_throws_when_feature_flag_disabled()
    {
        var sut = CreateSut(enableSso: false);

        var ex = await Assert.ThrowsAsync<NotImplementedException>(() =>
            sut.GetAuthorizationUrlAsync("google", "https://app.example/callback")
        );

        Assert.Contains("Feature-Flag", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAuthorizationUrlAsync_throws_provider_not_implemented_when_enabled()
    {
        var sut = CreateSut(enableSso: true);

        var ex = await Assert.ThrowsAsync<NotImplementedException>(() =>
            sut.GetAuthorizationUrlAsync("Google", "https://app.example/callback")
        );

        Assert.Contains("V1", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleCallbackAsync_throws_when_feature_flag_disabled()
    {
        var sut = CreateSut(enableSso: false);

        await Assert.ThrowsAsync<NotImplementedException>(() =>
            sut.HandleCallbackAsync("microsoft", new OidcCallbackRequest("code", "state"))
        );
    }

    private static OidcPlaceholderService CreateSut(bool enableSso)
    {
        var flags = Options.Create(new FeatureFlagsOptions { EnableOidcSso = enableSso });
        var oidc = Options.Create(new OidcOptions
        {
            Providers =
            {
                ["Google"] = new OidcProviderConfig
                {
                    Authority = "https://accounts.google.com",
                    CallbackPath = "/api/v1/auth/sso/google/callback",
                },
                ["Microsoft"] = new OidcProviderConfig
                {
                    Authority = "https://login.microsoftonline.com/common/v2.0",
                    CallbackPath = "/api/v1/auth/sso/microsoft/callback",
                },
            },
        });

        return new OidcPlaceholderService(flags, oidc);
    }
}
