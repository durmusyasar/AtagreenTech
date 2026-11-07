using AtaGreenTech.StockService.Domain.Localization;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.StockService.Application.Common;

/// <summary>
/// Sorgularda gösterilecek çeviriyi seçer: istenen dil, yoksa bölgesiz dil (en-US → en), yoksa sistemin varsayılan
/// dili, yoksa ilk çeviri.
/// </summary>
internal sealed class TranslationResolver
{
    private TranslationResolver(string language, string defaultLanguage)
    {
        Language = language;
        DefaultLanguage = defaultLanguage;
    }

    /// <summary>
    /// Normalize edilmiş istenen dil; istek dil belirtmiyorsa varsayılan dil.
    /// </summary>
    public string Language { get; }

    public string DefaultLanguage { get; }

    public static TranslationResolver For(string? requestedLanguage, IOptions<LocalizationOptions> localization)
    {
        var defaultLanguage = LanguageCode.Normalize(localization.Value.DefaultLanguage);
        var language = requestedLanguage is null ? defaultLanguage : LanguageCode.Normalize(requestedLanguage);
        return new TranslationResolver(language, defaultLanguage);
    }

    public T? Resolve<T>(IReadOnlyCollection<T> translations)
        where T : class, ITranslation
    {
        var baseLanguage = Language.Split('-')[0];

        return translations.FirstOrDefault(t => t.LanguageCode == Language)
            ?? translations.FirstOrDefault(t => t.LanguageCode == baseLanguage)
            ?? translations.FirstOrDefault(t => t.LanguageCode == DefaultLanguage)
            ?? translations.FirstOrDefault();
    }
}
