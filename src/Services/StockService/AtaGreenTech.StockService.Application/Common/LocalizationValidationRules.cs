using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Domain.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Common;

/// <summary>
/// Çok dilli komutların biçim kuralları (400). Çeviri setinin içerik kuralları domain'de (422).
/// </summary>
internal static class LocalizationValidationRules
{
    public static IRuleBuilderOptions<T, IReadOnlyList<TTranslation>> ValidTranslations<T, TTranslation>(
        this IRuleBuilder<T, IReadOnlyList<TTranslation>> rule,
        IErrorLocalizer localizer
    )
        where TTranslation : ITranslation =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Translations)
            .Must(HaveDistinctLanguages)
            .WithLocalizedMessage(localizer, ErrorCodes.ValidationDistinctLanguages);

    public static IRuleBuilderOptions<T, string?> ValidLanguageCode<T>(
        this IRuleBuilder<T, string?> rule,
        IErrorLocalizer localizer
    ) =>
        rule.MaximumLength(LanguageCode.MaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Language, LanguageCode.MaxLength)
            .Matches(LanguageCode.Pattern)
            .WithLocalizedMessage(localizer, ErrorCodes.ValidationLanguageCode);

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
