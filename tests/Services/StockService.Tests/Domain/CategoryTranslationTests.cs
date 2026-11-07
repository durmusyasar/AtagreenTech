using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Tests.Domain;

public class CategoryTranslationTests
{
    private const string DefaultLanguage = "tr";

    private static CategoryTranslation[] Translations(params (string Language, string Name, string? Description)[] items) =>
        items.Select(i => new CategoryTranslation(i.Language, i.Name, i.Description)).ToArray();

    [Fact]
    public void Creates_with_normalized_code_and_translations()
    {
        var parentId = Guid.NewGuid();

        var category = new Category(
            " seed ",
            Translations((" TR ", " Tohum ", "  "), ("en-gb", "Seeds", " Agricultural seeds ")),
            DefaultLanguage,
            parentId
        );

        Assert.Equal("SEED", category.Code);
        Assert.Equal(parentId, category.ParentCategoryId);
        Assert.True(category.IsActive);
        Assert.Equal(new[] { "tr", "en-GB" }, category.Translations.Select(t => t.LanguageCode));
        Assert.Equal(("Tohum", null), (category.Translations.First().Name, category.Translations.First().Description));
        Assert.Equal("Agricultural seeds", category.Translations.Last().Description);
    }

    [Fact]
    public void Requires_at_least_two_languages()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new Category("SEED", Translations(("tr", "Tohum", null)), DefaultLanguage)
        );

        Assert.Equal(LocalizationErrors.TranslationsBelowMinimum, exception.ResourceKey);
    }

    [Fact]
    public void Requires_default_language()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new Category("SEED", Translations(("en", "Seeds", null), ("de", "Saatgut", null)), DefaultLanguage)
        );

        Assert.Equal(LocalizationErrors.DefaultLanguageMissing, exception.ResourceKey);
    }

    [Fact]
    public void Rejects_duplicate_languages_after_normalization()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new Category("SEED", Translations(("tr", "Tohum", null), ("TR", "Tohumlar", null)), DefaultLanguage)
        );

        Assert.Equal(LocalizationErrors.DuplicateLanguage, exception.ResourceKey);
    }

    [Fact]
    public void Update_keeps_existing_rows_adds_new_and_removes_missing_languages()
    {
        var category = new Category("SEED", Translations(("tr", "Tohum", null), ("en", "Seeds", null)), DefaultLanguage);
        var turkish = category.Translations.Single(t => t.LanguageCode == "tr");
        var parentId = Guid.NewGuid();

        category.Update(
            "SEEDS",
            Translations(("tr", "Tohumlar", "Sertifikalı tohumlar"), ("de", "Saatgut", null)),
            DefaultLanguage,
            parentId,
            isActive: false
        );

        Assert.Equal("SEEDS", category.Code);
        Assert.Equal(parentId, category.ParentCategoryId);
        Assert.False(category.IsActive);
        Assert.Equal(new[] { "tr", "de" }, category.Translations.Select(t => t.LanguageCode));
        Assert.Same(turkish, category.Translations.Single(t => t.LanguageCode == "tr"));
        Assert.Equal(("Tohumlar", "Sertifikalı tohumlar"), (turkish.Name, turkish.Description));
    }

    [Fact]
    public void Failed_update_leaves_translations_unchanged()
    {
        var category = new Category("SEED", Translations(("tr", "Tohum", null), ("en", "Seeds", null)), DefaultLanguage);

        Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => category.Update("SEED", Translations(("en", "Seed", null)), DefaultLanguage, null, isActive: true)
        );

        Assert.Equal(new[] { "tr", "en" }, category.Translations.Select(t => t.LanguageCode));
        Assert.Equal("Seeds", category.Translations.Single(t => t.LanguageCode == "en").Name);
    }
}
