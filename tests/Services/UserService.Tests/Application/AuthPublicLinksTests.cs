using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class AuthPublicLinksTests
{
    [Fact]
    public void Build_adds_tenant_subdomain_on_multi_label_host()
    {
        var url = AuthPublicLinks.Build(
            new AuthOptions { PublicAppBaseUrl = "https://omnisync.com" },
            Tenant("atagreen"),
            "/verify-email",
            "tok+en"
        );

        Assert.Equal("https://atagreen.omnisync.com/verify-email?token=tok%2Ben", url);
    }

    [Fact]
    public void Build_skips_subdomain_on_localhost()
    {
        var url = AuthPublicLinks.Build(
            new AuthOptions { PublicAppBaseUrl = "http://localhost:5173" },
            Tenant("atagreen"),
            "reset-password",
            "abc"
        );

        Assert.Equal("http://localhost:5173/reset-password?token=abc", url);
    }

    [Fact]
    public void Build_does_not_double_prefix_existing_tenant_host()
    {
        var url = AuthPublicLinks.Build(
            new AuthOptions { PublicAppBaseUrl = "https://atagreen.omnisync.com" },
            Tenant("atagreen"),
            "/accept-invite",
            "x"
        );

        Assert.Equal("https://atagreen.omnisync.com/accept-invite?token=x", url);
    }

    [Fact]
    public void Build_works_without_tenant_identifier()
    {
        var url = AuthPublicLinks.Build(
            new AuthOptions { PublicAppBaseUrl = "https://omnisync.com/" },
            Tenant(null),
            "/verify-email",
            "t"
        );

        Assert.Equal("https://omnisync.com/verify-email?token=t", url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Build_rejects_blank_path(string? path)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AuthPublicLinks.Build(
                new AuthOptions { PublicAppBaseUrl = "https://omnisync.com" },
                Tenant("atagreen"),
                path!,
                "t"
            )
        );
        Assert.StartsWith(ErrorCodes.ValidationRequired, ex.Message, StringComparison.Ordinal);
        Assert.Equal("path", ex.ParamName);
    }

    [Fact]
    public void Build_rejects_invalid_base_url()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            AuthPublicLinks.Build(
                new AuthOptions { PublicAppBaseUrl = "not-a-url" },
                Tenant("atagreen"),
                "/x",
                "t"
            )
        );
        Assert.StartsWith(ErrorCodes.ValidationInvalid, ex.Message, StringComparison.Ordinal);
        Assert.Equal("PublicAppBaseUrl", ex.ParamName);
    }

    private static FakeCurrentTenant Tenant(string? identifier) =>
        new()
        {
            Id = identifier is null ? null : Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
            IsAvailable = identifier is not null,
            Identifier = identifier,
        };
}
