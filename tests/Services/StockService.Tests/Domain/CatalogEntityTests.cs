using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Tests.Domain;

public class CatalogEntityTests
{
    [Fact]
    public void Product_normalizes_sku_and_text_fields()
    {
        var product = new Product("  Damla Sulama Borusu ", " dsb-16mm ", "   ", 12.5m, Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal("Damla Sulama Borusu", product.Name);
        Assert.Equal("DSB-16MM", product.Sku);
        Assert.Null(product.Description);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Product_rejects_negative_price()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Product("Vana", "V-1", null, -1m, Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [Fact]
    public void UnitOfMeasure_normalizes_code()
    {
        UnitOfMeasureTranslation[] translations = [new("tr", "Kilogram", "kg"), new("en", "Kilogram", "kg")];

        Assert.Equal("KG", new UnitOfMeasure(" kg ", translations, "tr").Code);
    }

    [Fact]
    public void Category_cannot_be_its_own_parent()
    {
        CategoryTranslation[] Translations() => [new("tr", "Sulama", null), new("en", "Irrigation", null)];
        var category = new Category("IRRIGATION", Translations(), "tr");

        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => category.Update("IRRIGATION", Translations(), "tr", category.Id, isActive: true)
        );

        Assert.Equal(CatalogErrors.CategoryCannotBeItsOwnParent, exception.ResourceKey);
    }
}
