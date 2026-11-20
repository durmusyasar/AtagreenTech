using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Categories;

internal static class CategoryValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidCategoryCode<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Code)
            .MaximumLength(Category.CodeMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Code, Category.CodeMaxLength);

    public static IRuleBuilderOptions<T, Guid?> ValidParentCategoryId<T>(
        this IRuleBuilder<T, Guid?> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEqual(Guid.Empty)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationInvalid,
                localizer.Localize(ErrorCodes.Fields.ParentCategory)
            );
}

internal sealed class CategoryTranslationDtoValidator : AbstractValidator<CategoryTranslationDto>
{
    public CategoryTranslationDtoValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Language)
            .ValidLanguageCode(localizer);

        RuleFor(x => x.Name)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Name)
            .MaximumLength(CategoryTranslation.NameMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Name, CategoryTranslation.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(CategoryTranslation.DescriptionMaxLength)
            .WithMaxLengthMessage(
                localizer,
                ErrorCodes.Fields.Description,
                CategoryTranslation.DescriptionMaxLength
            );
    }
}
