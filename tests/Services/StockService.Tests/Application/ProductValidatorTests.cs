using AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;
using AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;
using AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Tests.Application;

public class ProductValidatorTests
{
    private static CreateProductCommand ValidCommand() =>
        new(
            "DSB-16MM",
            "4006381333931",
            12.5m,
            "TRY",
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new("tr", "Damla Sulama Borusu", "16 mm"), new("en", "Drip Irrigation Pipe", null)]
        );

    [Fact]
    public void Valid_create_command_passes()
    {
        Assert.True(new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand()).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Barcode_is_optional(string? barcode)
    {
        var command = ValidCommand() with { Barcode = barcode };

        Assert.True(new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(command).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("DSB 16")]
    public void Invalid_sku_fails(string sku)
    {
        var result = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand() with { Sku = sku });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Theory]
    [InlineData("4006381333932")]
    [InlineData("400638133393")]
    [InlineData("ABC-123")]
    public void Invalid_barcode_fails(string barcode)
    {
        var result = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand() with { Barcode = barcode });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Barcode));
    }

    [Theory]
    [InlineData("")]
    [InlineData("TL")]
    [InlineData("GBP")]
    public void Unsupported_currency_fails(string currency)
    {
        var result = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand() with { Currency = currency });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.Currency));
    }

    [Fact]
    public void Currency_is_case_insensitive()
    {
        Assert.True(new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand() with { Currency = "usd" }).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1.005)]
    public void Non_positive_or_over_precise_unit_cost_fails(decimal unitPrice)
    {
        var result = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand() with { UnitPrice = unitPrice });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateProductCommand.UnitPrice));
    }

    [Fact]
    public void Missing_references_fail()
    {
        var command = ValidCommand() with { CategoryId = Guid.Empty, UnitOfMeasureId = Guid.Empty };

        var failedProperties = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(command).Errors.Select(e => e.PropertyName);

        Assert.Equivalent(
            new[] { nameof(CreateProductCommand.CategoryId), nameof(CreateProductCommand.UnitOfMeasureId) },
            failedProperties
        );
    }

    [Fact]
    public void Too_long_translated_name_fails()
    {
        var command = ValidCommand() with
        {
            Translations = [new("tr", new string('a', ProductTranslation.NameMaxLength + 1), null), new("en", "Pipe", null)],
        };

        var result = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == "Translations[0].Name");
    }

    [Fact]
    public void Empty_and_duplicate_translations_fail()
    {
        var empty = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(ValidCommand() with { Translations = [] });
        var duplicate = new CreateProductCommandValidator(TestLocalizers.Passthrough).Validate(
            ValidCommand() with { Translations = [new("tr", "Boru", null), new(" TR ", "Hortum", null)] }
        );

        Assert.Contains(empty.Errors, e => e.PropertyName == nameof(CreateProductCommand.Translations));
        Assert.Contains(duplicate.Errors, e => e.PropertyName == nameof(CreateProductCommand.Translations));
    }

    [Fact]
    public void Update_command_requires_id_and_uses_same_rules()
    {
        var command = new UpdateProductCommand(
            Guid.Empty,
            "DSB-16MM",
            "123",
            12.5m,
            "EUR",
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new("tr", "Damla Sulama Borusu", null), new("en", "Drip Irrigation Pipe", null)],
            IsActive: true
        );

        var failedProperties = new UpdateProductCommandValidator(TestLocalizers.Passthrough).Validate(command).Errors.Select(e => e.PropertyName);

        Assert.Equivalent(
            new[] { nameof(UpdateProductCommand.Id), nameof(UpdateProductCommand.Barcode) },
            failedProperties
        );
    }

    [Fact]
    public void List_query_rejects_invalid_language()
    {
        var result = new GetProductsQueryValidator(TestLocalizers.Passthrough).Validate(new GetProductsQuery(Language: "english"));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetProductsQuery.Language));
    }
}
