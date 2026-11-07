using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure;

/// <summary>
/// Ölçü birimi komutlarının biçim kuralları (400). Çeviri setinin içerik kuralları (en az iki dil, varsayılan dilin
/// bulunması) domain'de iş kuralı olarak (422) uygulanır.
/// </summary>
internal static class UnitOfMeasureValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidUnitOfMeasureCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Birim kodu boş olamaz.")
            .MaximumLength(UnitOfMeasure.CodeMaxLength)
            .WithMessage($"Birim kodu en fazla {UnitOfMeasure.CodeMaxLength} karakter olabilir.");

    public static IRuleBuilderOptions<T, IReadOnlyList<UnitOfMeasureTranslationDto>> ValidTranslations<T>(
        this IRuleBuilder<T, IReadOnlyList<UnitOfMeasureTranslationDto>> rule
    ) =>
        rule.NotEmpty().WithMessage("Çeviri bilgisi boş olamaz.")
            .Must(HaveDistinctLanguages).WithMessage("Aynı dil için birden fazla çeviri girilemez.");

    public static IRuleBuilderOptions<T, string?> ValidLanguageCode<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(LanguageCode.MaxLength)
            .WithMessage($"Dil kodu en fazla {LanguageCode.MaxLength} karakter olabilir.")
            .Matches(LanguageCode.Pattern)
            .WithMessage("Dil kodu 'tr', 'en' veya 'en-US' biçiminde olmalıdır.");

    private static bool HaveDistinctLanguages(IReadOnlyList<UnitOfMeasureTranslationDto> translations)
    {
        var codes = translations
            .Where(t => !string.IsNullOrWhiteSpace(t?.LanguageCode))
            .Select(t => LanguageCode.Normalize(t.LanguageCode))
            .ToList();

        return codes.Count == codes.Distinct(StringComparer.Ordinal).Count();
    }
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
