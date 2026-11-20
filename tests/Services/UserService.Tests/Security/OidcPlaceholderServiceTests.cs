using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Infrastructure.Security;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class OidcPlaceholderServiceTests
{
    [Fact]
    public async Task GetAuthorizationUrlAsync_throws_sso_not_implemented()
    {
        var sut = new OidcPlaceholderService();

        var ex = await Assert.ThrowsAsync<NotImplementedException>(() =>
            sut.GetAuthorizationUrlAsync("google", "https://app.example/callback")
        );

        Assert.Equal(ErrorCodes.SsoNotImplemented, ex.Message);
    }

    [Fact]
    public async Task HandleCallbackAsync_throws_sso_not_implemented()
    {
        var sut = new OidcPlaceholderService();

        var ex = await Assert.ThrowsAsync<NotImplementedException>(() =>
            sut.HandleCallbackAsync("microsoft", new OidcCallbackRequest("code", "state"))
        );

        Assert.Equal(ErrorCodes.SsoNotImplemented, ex.Message);
    }

    [Fact]
    public async Task HandleCallbackAsync_throws_when_request_null()
    {
        var sut = new OidcPlaceholderService();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.HandleCallbackAsync("microsoft", null!)
        );
    }
}
