namespace AtaGreenTech.StockService.Domain.Localization;

/// <summary>
/// Çeviri dil kodları: ISO 639-1 dil kodu, isteğe bağlı olarak ISO 3166-1 bölge koduyla (ör. <c>tr</c>, <c>en</c>,
/// <c>en-US</c>).
/// </summary>
public static class LanguageCode
{
    public const int MaxLength = 5;
    public const string Pattern = "^[A-Za-z]{2}(-[A-Za-z]{2})?$";

    /// <summary>
    /// Dil kısmını küçük, bölge kısmını büyük harfe çevirir (ör. <c>EN-us</c> → <c>en-US</c>).
    /// </summary>
    public static string Normalize(string languageCode)
    {
        var parts = languageCode.Trim().Split('-');
        return parts.Length == 2
            ? $"{parts[0].ToLowerInvariant()}-{parts[1].ToUpperInvariant()}"
            : parts[0].ToLowerInvariant();
    }
}
