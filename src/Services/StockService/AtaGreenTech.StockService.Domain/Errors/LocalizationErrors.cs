namespace AtaGreenTech.StockService.Domain.Errors;

/// <summary>
/// Çok dilli kayıtların çeviri kuralı ihlallerinin resource key'leri; OmniSync bunları 422 yanıtının
/// <c>resourceKey</c> alanında döndürür.
/// </summary>
public static class LocalizationErrors
{
    public const string DuplicateLanguage = "Localization.DuplicateLanguage";
    public const string TranslationsBelowMinimum = "Localization.TranslationsBelowMinimum";
    public const string DefaultLanguageMissing = "Localization.DefaultLanguageMissing";
}
