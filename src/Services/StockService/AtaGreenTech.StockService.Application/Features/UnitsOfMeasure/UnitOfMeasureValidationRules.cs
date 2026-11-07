using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure;

internal static class UnitOfMeasureValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidUnitOfMeasureCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Birim kodu boş olamaz.")
            .MaximumLength(UnitOfMeasure.CodeMaxLength)
            .WithMessage($"Birim kodu en fazla {UnitOfMeasure.CodeMaxLength} karakter olabilir.");
}

internal sealed class UnitOfMeasureTranslationDtoValidator : AbstractValidator<UnitOfMeasureTranslationDto>
{
    public UnitOfMeasureTranslationDtoValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu boş olamaz.")
            .ValidLanguageCode();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ölçü birimi adı boş olamaz.")
            .MaximumLength(UnitOfMeasureTranslation.NameMaxLength)
            .WithMessage($"Ölçü birimi adı en fazla {UnitOfMeasureTranslation.NameMaxLength} karakter olabilir.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Birim sembolü boş olamaz.")
            .MaximumLength(UnitOfMeasureTranslation.SymbolMaxLength)
            .WithMessage($"Birim sembolü en fazla {UnitOfMeasureTranslation.SymbolMaxLength} karakter olabilir.");
    }
}
