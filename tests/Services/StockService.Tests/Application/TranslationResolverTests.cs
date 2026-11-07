using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.StockService.Tests.Application;

public class TranslationResolverTests
{
    private static readonly CategoryTranslationDto[] Translations =
    [
        new("de", "Saatgut", null),
        new("en", "Seeds", null),
        new("tr", "Tohum", null),
    ];

    private static TranslationResolver Resolver(string? language) =>
        TranslationResolver.For(language, Options.Create(new LocalizationOptions { DefaultLanguage = "TR" }));

    [Theory]
    [InlineData("en", "Seeds")]
    [InlineData("EN-gb", "Seeds")]
    [InlineData("fr", "Tohum")]
    [InlineData(null, "Tohum")]
    public void Resolves_requested_then_base_then_default_language(string? language, string expected)
    {
        Assert.Equal(expected, Resolver(language).Resolve(Translations)?.Name);
    }

    [Fact]
    public void Falls_back_to_first_translation_when_default_language_is_missing()
    {
        Assert.Equal("Saatgut", Resolver("fr").Resolve(Translations[..2])?.Name);
    }

    [Fact]
    public void Returns_null_without_translations()
    {
        Assert.Null(Resolver("en").Resolve(Array.Empty<CategoryTranslationDto>()));
    }
}
