using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Tests.Domain;

public class CatalogEntityTests
{
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
