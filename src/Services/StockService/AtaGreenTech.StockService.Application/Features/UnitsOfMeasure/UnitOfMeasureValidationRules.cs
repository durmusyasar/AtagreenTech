using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure;

internal static class UnitOfMeasureValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidUnitOfMeasureCode<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Code)
            .MaximumLength(UnitOfMeasure.CodeMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Code, UnitOfMeasure.CodeMaxLength);
}

internal sealed class UnitOfMeasureTranslationDtoValidator : AbstractValidator<UnitOfMeasureTranslationDto>
{
    public UnitOfMeasureTranslationDtoValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Language)
            .ValidLanguageCode(localizer);

        RuleFor(x => x.Name)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Name)
            .MaximumLength(UnitOfMeasureTranslation.NameMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Name, UnitOfMeasureTranslation.NameMaxLength);

        RuleFor(x => x.Symbol)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Symbol)
            .MaximumLength(UnitOfMeasureTranslation.SymbolMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Symbol, UnitOfMeasureTranslation.SymbolMaxLength);
    }
}
