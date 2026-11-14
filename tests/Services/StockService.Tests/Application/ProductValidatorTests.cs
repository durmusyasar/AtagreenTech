using AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Tests.Application;

public class ProductValidatorTests
{
    private static CreateProductCommand ValidCommand() =>
        new("Damla Sulama Borusu", "DSB-16MM", null, 12.5m, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Valid_create_command_passes()
    {
        Assert.True(new CreateProductCommandValidator().Validate(ValidCommand()).IsValid);
    }

    [Theory]
    [InlineData("", nameof(CreateProductCommand.Sku))]
    [InlineData("DSB 16", nameof(CreateProductCommand.Sku))]
    public void Invalid_sku_fails(string sku, string expectedProperty)
    {
        var result = new CreateProductCommandValidator().Validate(ValidCommand() with { Sku = sku });

        Assert.Contains(result.Errors, e => e.PropertyName == expectedProperty);
    }

    [Fact]
    public void Too_long_name_fails()
    {
        var command = ValidCommand() with { Name = new string('a', Product.NameMaxLength + 1) };

        var result = new CreateProductCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Missing_references_and_non_positive_price_fail()
    {
        var command = ValidCommand() with { UnitPrice = 0, CategoryId = Guid.Empty, UnitOfMeasureId = Guid.Empty };

        var failedProperties = new CreateProductCommandValidator().Validate(command).Errors.Select(e => e.PropertyName);

        Assert.Equivalent(
            new[]
            {
                nameof(CreateProductCommand.UnitPrice),
                nameof(CreateProductCommand.CategoryId),
                nameof(CreateProductCommand.UnitOfMeasureId),
            },
            failedProperties
        );
    }
}
