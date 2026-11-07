using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Localization;

/// <summary>
/// Çok dilli kayıtların çeviri setine ait iş kuralları (422): her dil en fazla bir kez, en az
/// <see cref="MinimumTranslationCount"/> farklı dil ve sistemin varsayılan dili mutlaka bulunur.
/// </summary>
public static class TranslationSetGuard
{
    public const int MinimumTranslationCount = 2;

    /// <param name="languageCodes">Normalize edilmiş dil kodları.</param>
    /// <param name="defaultLanguage">Sistemin varsayılan (aktif) dili.</param>
    public static void Ensure(IReadOnlyCollection<string> languageCodes, string defaultLanguage)
    {
        var distinctCount = languageCodes.Distinct(StringComparer.Ordinal).Count();
        var normalizedDefault = LanguageCode.Normalize(defaultLanguage);

        CheckRule.ThrowIf(
            new TranslationLanguagesMustBeUniqueRule(languageCodes.Count, distinctCount),
            LocalizationErrors.DuplicateLanguage
        );
        CheckRule.ThrowIf(
            new MinimumTranslationsRule(distinctCount, MinimumTranslationCount),
            LocalizationErrors.TranslationsBelowMinimum,
            MinimumTranslationCount
        );
        CheckRule.ThrowIf(
            new DefaultLanguageTranslationRequiredRule(languageCodes.Contains(normalizedDefault, StringComparer.Ordinal)),
            LocalizationErrors.DefaultLanguageMissing,
            normalizedDefault
        );
    }

    private sealed class TranslationLanguagesMustBeUniqueRule(int count, int distinctCount) : IBusinessRule
    {
        public bool IsBroken() => count != distinctCount;
    }

    private sealed class MinimumTranslationsRule(int distinctCount, int minimum) : IBusinessRule
    {
        public bool IsBroken() => distinctCount < minimum;
    }

    private sealed class DefaultLanguageTranslationRequiredRule(bool containsDefaultLanguage) : IBusinessRule
    {
        public bool IsBroken() => !containsDefaultLanguage;
    }
}
