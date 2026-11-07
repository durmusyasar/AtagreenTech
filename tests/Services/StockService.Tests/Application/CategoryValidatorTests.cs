using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;
using AtaGreenTech.StockService.Application.Features.Categories.Commands.UpdateCategory;
using AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;

namespace AtaGreenTech.StockService.Tests.Application;

public class CategoryValidatorTests
{
    private static CreateCategoryCommand ValidCommand() =>
        new("SEED", [new("tr", "Tohum", "Tarımsal tohum çeşitleri"), new("en", "Seeds", null)]);

    [Fact]
    public void Valid_command_passes()
    {
        Assert.True(new CreateCategoryCommandValidator().Validate(ValidCommand()).IsValid);
    }

    [Fact]
    public void Single_translation_passes_format_validation_and_is_left_to_domain_rules()
    {
        var command = ValidCommand() with { Translations = [new("en", "Seeds", "Agricultural seeds")] };

        Assert.True(new CreateCategoryCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Missing_code_and_empty_translations_fail()
    {
        var command = new CreateCategoryCommand("", []);

        var result = new CreateCategoryCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Code));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Translations));
    }

    [Fact]
    public void Duplicate_languages_fail()
    {
        var command = ValidCommand() with { Translations = [new("tr", "Tohum", null), new(" TR ", "Tohumlar", null)] };

        var result = new CreateCategoryCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCategoryCommand.Translations));
    }

    [Theory]
    [InlineData("", "Seeds", "LanguageCode")]
    [InlineData("english", "Seeds", "LanguageCode")]
    [InlineData("en", "", "Name")]
    public void Invalid_translation_fields_fail(string language, string name, string property)
    {
        var command = ValidCommand() with { Translations = [new("tr", "Tohum", null), new(language, name, null)] };

        var result = new CreateCategoryCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == $"Translations[1].{property}");
    }

    [Fact]
    public void Too_long_name_and_description_fail()
    {
        var command = ValidCommand() with
        {
            Translations =
            [
                new("tr", "Tohum", null),
                new CategoryTranslationDto("en", new string('n', 151), new string('d', 501)),
            ],
        };

        var result = new CreateCategoryCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == "Translations[1].Name");
        Assert.Contains(result.Errors, e => e.PropertyName == "Translations[1].Description");
    }

    [Fact]
    public void Empty_parent_category_id_fails()
    {
        var command = ValidCommand() with { ParentCategoryId = Guid.Empty };

        Assert.False(new CreateCategoryCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void Update_command_applies_the_same_translation_rules()
    {
        var command = new UpdateCategoryCommand(
            Guid.NewGuid(),
            "SEED",
            [new("tr", "Tohum", null), new("tr", "Tohumlar", null)],
            null,
            IsActive: true
        );

        var result = new UpdateCategoryCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCategoryCommand.Translations));
    }

    [Theory]
    [InlineData("en", true)]
    [InlineData("en-US", true)]
    [InlineData("english", false)]
    public void Query_language_is_validated(string language, bool isValid)
    {
        var query = new GetCategoriesQuery(Language: language);

        Assert.Equal(isValid, new GetCategoriesQueryValidator().Validate(query).IsValid);
    }
}
