using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Tests.Domain;

public class ProductTests
{
    private const string DefaultLanguage = "tr";

    private static ProductTranslation[] Translations(params (string Language, string Name, string? Description)[] items) =>
        items.Select(i => new ProductTranslation(i.Language, i.Name, i.Description)).ToArray();

    private static Product NewProduct() =>
        new(
            "DSB-16MM",
            null,
            12.5m,
            "TRY",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Translations(("tr", "Damla Sulama Borusu", null), ("en", "Drip Irrigation Pipe", null)),
            DefaultLanguage
        );

    [Fact]
    public void Creates_with_normalized_fields_and_translations()
    {
        var product = new Product(
            " dsb-16mm ",
            " 4006381333931 ",
            12.5m,
            " usd ",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Translations((" TR ", " Damla Sulama Borusu ", "   "), ("en-gb", "Drip Pipe", " 16 mm ")),
            DefaultLanguage
        );

        Assert.Equal("DSB-16MM", product.Sku);
        Assert.Equal("4006381333931", product.Barcode);
        Assert.Equal("USD", product.Currency);
        Assert.True(product.IsActive);
        Assert.Equal(new[] { "tr", "en-GB" }, product.Translations.Select(t => t.LanguageCode));
        Assert.Equal(("Damla Sulama Borusu", null), (product.Translations.First().Name, product.Translations.First().Description));
        Assert.Equal("16 mm", product.Translations.Last().Description);
    }

    [Fact]
    public void Blank_barcode_is_stored_as_null()
    {
        var product = NewProduct();

        product.Update(
            product.Sku,
            "  ",
            product.UnitPrice,
            product.Currency,
            product.CategoryId,
            product.UnitOfMeasureId,
            Translations(("tr", "Boru", null), ("en", "Pipe", null)),
            DefaultLanguage,
            isActive: true
        );

        Assert.Null(product.Barcode);
    }

    [Fact]
    public void Rejects_negative_price()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Product(
                "V-1",
                null,
                -1m,
                "TRY",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Translations(("tr", "Vana", null), ("en", "Valve", null)),
                DefaultLanguage
            )
        );
    }

    [Fact]
    public void Requires_default_language_translation()
    {
        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => new Product(
                "V-1",
                null,
                1m,
                "TRY",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Translations(("en", "Valve", null), ("de", "Ventil", null)),
                DefaultLanguage
            )
        );

        Assert.Equal(LocalizationErrors.DefaultLanguageMissing, exception.ResourceKey);
    }

    [Fact]
    public void Update_replaces_translations_and_details()
    {
        var product = NewProduct();
        var turkish = product.Translations.Single(t => t.LanguageCode == "tr");
        var categoryId = Guid.NewGuid();

        product.Update(
            "DSB-20MM",
            "96385074",
            15m,
            "EUR",
            categoryId,
            product.UnitOfMeasureId,
            Translations(("tr", "Damla Sulama Borusu 20 mm", "Yeni"), ("de", "Tropfrohr", null)),
            DefaultLanguage,
            isActive: false
        );

        Assert.Equal(("DSB-20MM", "96385074", 15m, "EUR"), (product.Sku, product.Barcode, product.UnitPrice, product.Currency));
        Assert.Equal(categoryId, product.CategoryId);
        Assert.False(product.IsActive);
        Assert.Equal(new[] { "tr", "de" }, product.Translations.Select(t => t.LanguageCode));
        Assert.Same(turkish, product.Translations.Single(t => t.LanguageCode == "tr"));
        Assert.Equal(("Damla Sulama Borusu 20 mm", "Yeni"), (turkish.Name, turkish.Description));
    }

    [Fact]
    public void Failed_update_leaves_product_unchanged()
    {
        var product = NewProduct();

        Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => product.Update(
                "OTHER",
                null,
                99m,
                "USD",
                product.CategoryId,
                product.UnitOfMeasureId,
                Translations(("en", "Pipe", null)),
                DefaultLanguage,
                isActive: false
            )
        );

        Assert.Equal(("DSB-16MM", 12.5m, "TRY", true), (product.Sku, product.UnitPrice, product.Currency, product.IsActive));
        Assert.Equal(new[] { "tr", "en" }, product.Translations.Select(t => t.LanguageCode));
    }
}
