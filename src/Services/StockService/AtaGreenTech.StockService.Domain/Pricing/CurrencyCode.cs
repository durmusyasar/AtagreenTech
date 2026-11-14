namespace AtaGreenTech.StockService.Domain.Pricing;

/// <summary>
/// Ürün maliyetlerinin para birimi: desteklenen ISO 4217 kodlarından biri.
/// </summary>
public static class CurrencyCode
{
    public const int Length = 3;
    public const string Default = "TRY";

    public static readonly IReadOnlyList<string> Supported = ["TRY", "USD", "EUR"];

    /// <summary>
    /// Büyük harfe çevirir (ör. <c>usd</c> → <c>USD</c>).
    /// </summary>
    public static string Normalize(string currency) => currency.Trim().ToUpperInvariant();

    public static bool IsSupported(string? currency) => currency is not null && Supported.Contains(Normalize(currency));
}
