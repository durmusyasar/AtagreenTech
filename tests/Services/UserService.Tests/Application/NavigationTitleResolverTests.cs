using AtaGreenTech.UserService.Application.Features.Navigation;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class NavigationTitleResolverTests
{
    [Theory]
    [InlineData("tr", "Türkçe")]
    [InlineData("en-US", "English")]
    [InlineData("fr", "Türkçe")]
    public void Resolve_prefers_requested_then_default(string preferred, string expected)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tr"] = "Türkçe",
            ["en"] = "English",
        };

        Assert.Equal(
            expected,
            NavigationTitleResolver.Resolve(map, preferred, "tr", "stock.units")
        );
    }

    [Fact]
    public void Resolve_returns_code_when_translations_empty()
    {
        Assert.Equal(
            "stock.units",
            NavigationTitleResolver.Resolve(
                new Dictionary<string, string>(),
                "tr",
                "tr",
                "stock.units"
            )
        );
    }

    [Theory]
    [InlineData("tr-TR,tr;q=0.9,en;q=0.8", "tr")]
    [InlineData("en", "en")]
    [InlineData(" EN-us ", "en")]
    public void Normalize_extracts_primary_language(string raw, string expected)
    {
        Assert.Equal(expected, NavigationTitleResolver.Normalize(raw));
    }
}
