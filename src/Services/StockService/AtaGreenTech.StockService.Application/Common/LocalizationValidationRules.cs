using AtaGreenTech.StockService.Domain.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Common;

/// <summary>
/// Çok dilli komutların biçim kuralları (400). Çeviri setinin içerik kuralları (en az iki dil, varsayılan dilin
/// bulunması) domain'de iş kuralı olarak (422) uygulanır.
/// </summary>
internal static class LocalizationValidationRules
{
    public static IRuleBuilderOptions<T, IReadOnlyList<TTranslation>> ValidTranslations<T, TTranslation>(
        this IRuleBuilder<T, IReadOnlyList<TTranslation>> rule
    )
        where TTranslation : ITranslation =>
        rule.NotEmpty().WithMessage("Çeviri bilgisi boş olamaz.")
            .Must(HaveDistinctLanguages).WithMessage("Aynı dil için birden fazla çeviri girilemez.");

    public static IRuleBuilderOptions<T, string?> ValidLanguageCode<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(LanguageCode.MaxLength)
            .WithMessage($"Dil kodu en fazla {LanguageCode.MaxLength} karakter olabilir.")
            .Matches(LanguageCode.Pattern)
            .WithMessage("Dil kodu 'tr', 'en' veya 'en-US' biçiminde olmalıdır.");

    private static bool HaveDistinctLanguages<TTranslation>(IReadOnlyList<TTranslation> translations)
        where TTranslation : ITranslation
    {
        var codes = translations
            .Where(t => !string.IsNullOrWhiteSpace(t?.LanguageCode))
            .Select(t => LanguageCode.Normalize(t.LanguageCode))
            .ToList();

        return codes.Count == codes.Distinct(StringComparer.Ordinal).Count();
    }
}
