using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Tests.Domain;

public class UnitOfMeasureTranslationTests
{
    private const string DefaultLanguage = "tr";

    private static UnitOfMeasureTranslation[] Translations(params (string Language, string Name, string Symbol)[] items) =>
        items.Select(i => new UnitOfMeasureTranslation(i.Language, i.Name, i.Symbol)).ToArray();

    [Fact]
    public void Creates_with_normalized_translations()
    {
        var unit = new UnitOfMeasure(
            "kg",
            Translations((" TR ", " Kilogram ", " kg "), ("en-us", "Kilogram", "kg")),
            DefaultLanguage
        );

        Assert.Equal(new[] { "tr", "en-US" }, unit.Translations.Select(t => t.LanguageCode));
        Assert.Equal(("Kilogram", "kg"), (unit.Translations.First().Name, unit.Translations.First().Symbol));
        Assert.True(unit.IsActive);
    }

    [Fact]
    public void Requires_at_least_two_languages()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new UnitOfMeasure("KG", Translations(("tr", "Kilogram", "kg")), DefaultLanguage)
        );

        Assert.Equal(LocalizationErrors.TranslationsBelowMinimum, exception.ResourceKey);
    }

    [Fact]
    public void Requires_default_language()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new UnitOfMeasure("KG", Translations(("en", "Kilogram", "kg"), ("de", "Kilogramm", "kg")), DefaultLanguage)
        );

        Assert.Equal(LocalizationErrors.DefaultLanguageMissing, exception.ResourceKey);
    }

    [Fact]
    public void Rejects_duplicate_languages_after_normalization()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new UnitOfMeasure("KG", Translations(("tr", "Kilogram", "kg"), ("TR", "Kilo", "kg")), DefaultLanguage)
        );

        Assert.Equal(LocalizationErrors.DuplicateLanguage, exception.ResourceKey);
    }

    [Fact]
    public void Update_keeps_existing_rows_adds_new_and_removes_missing_languages()
    {
        var unit = new UnitOfMeasure("AD", Translations(("tr", "Adet", "ad"), ("en", "Piece", "pc")), DefaultLanguage);
        var turkish = unit.Translations.Single(t => t.LanguageCode == "tr");

        unit.Update("ADET", Translations(("tr", "Adet", "adet"), ("de", "Stück", "Stk")), DefaultLanguage, isActive: false);

        Assert.Equal("ADET", unit.Code);
        Assert.False(unit.IsActive);
        Assert.Equal(new[] { "tr", "de" }, unit.Translations.Select(t => t.LanguageCode));
        Assert.Same(turkish, unit.Translations.Single(t => t.LanguageCode == "tr"));
        Assert.Equal("adet", turkish.Symbol);
    }

    [Fact]
    public void Failed_update_leaves_translations_unchanged()
    {
        var unit = new UnitOfMeasure("AD", Translations(("tr", "Adet", "ad"), ("en", "Piece", "pc")), DefaultLanguage);

        Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => unit.Update("AD", Translations(("en", "Piece", "pcs")), DefaultLanguage, isActive: true)
        );

        Assert.Equal(new[] { "tr", "en" }, unit.Translations.Select(t => t.LanguageCode));
        Assert.Equal("pc", unit.Translations.Single(t => t.LanguageCode == "en").Symbol);
    }
}
