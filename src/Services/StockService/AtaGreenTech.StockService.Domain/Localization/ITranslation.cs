namespace AtaGreenTech.StockService.Domain.Localization;

/// <summary>
/// Çok dilli bir kaydın tek bir dildeki metinleri.
/// </summary>
public interface ITranslation
{
    /// <summary>
    /// Normalize edilmiş dil kodu (ör. <c>tr</c>, <c>en-US</c>).
    /// </summary>
    string LanguageCode { get; }
}
