using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;
using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

namespace AtaGreenTech.StockService.Tests.Application;

public class UnitOfMeasureValidatorTests
{
    private static CreateUnitOfMeasureCommand ValidCommand() =>
        new("KG", [new("tr", "Kilogram", "kg"), new("en", "Kilogram", "kg")]);

    [Fact]
    public void Valid_command_passes()
    {
        Assert.True(new CreateUnitOfMeasureCommandValidator().Validate(ValidCommand()).IsValid);
    }

    [Fact]
    public void Single_translation_passes_format_validation_and_is_left_to_domain_rules()
    {
        var command = ValidCommand() with { Translations = [new("en", "Kilogram", "kg")] };

        Assert.True(new CreateUnitOfMeasureCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Empty_translations_fail()
    {
        var command = ValidCommand() with { Translations = [] };

        var result = new CreateUnitOfMeasureCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUnitOfMeasureCommand.Translations));
    }

    [Fact]
    public void Duplicate_languages_fail()
    {
        var command = ValidCommand() with { Translations = [new("tr", "Kilogram", "kg"), new(" TR ", "Kilo", "kg")] };

        var result = new CreateUnitOfMeasureCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUnitOfMeasureCommand.Translations));
    }

    [Theory]
    [InlineData("", "Kilogram", "kg", "LanguageCode")]
    [InlineData("turkish", "Kilogram", "kg", "LanguageCode")]
    [InlineData("t1", "Kilogram", "kg", "LanguageCode")]
    [InlineData("en", "", "kg", "Name")]
    [InlineData("en", "Kilogram", "", "Symbol")]
    public void Invalid_translation_fields_fail(string language, string name, string symbol, string property)
    {
        var command = ValidCommand() with { Translations = [new("tr", "Kilogram", "kg"), new(language, name, symbol)] };

        var result = new CreateUnitOfMeasureCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == $"Translations[1].{property}");
    }

    [Fact]
    public void Too_long_symbol_fails()
    {
        var command = ValidCommand() with
        {
            Translations = [new("tr", "Kilogram", "kg"), new UnitOfMeasureTranslationDto("en", "Kilogram", new string('k', 21))],
        };

        Assert.False(new CreateUnitOfMeasureCommandValidator().Validate(command).IsValid);
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("en-US", true)]
    [InlineData("english", false)]
    public void Query_language_is_validated(string language, bool isValid)
    {
        Assert.Equal(isValid, new GetUnitsOfMeasureQueryValidator().Validate(new GetUnitsOfMeasureQuery(language)).IsValid);
    }
}
