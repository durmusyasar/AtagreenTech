namespace AtaGreenTech.StockService.Domain.Pricing;

/// <summary>
/// GS1 ürün barkodları (GTIN): EAN-8, UPC-A (12 hane), EAN-13 ve GTIN-14. Son hane, diğer hanelerden mod 10 ile
/// hesaplanan kontrol hanesidir.
/// </summary>
public static class Gtin
{
    public const int MaxLength = 14;

    public static readonly IReadOnlyList<int> Lengths = [8, 12, 13, 14];

    /// <summary>
    /// Boşlukları kırpar; boş barkod <see langword="null"/> olur.
    /// </summary>
    public static string? Normalize(string? barcode) => string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();

    public static bool IsValid(string? barcode)
    {
        var value = barcode?.Trim();
        if (value is null || !Lengths.Contains(value.Length) || !value.All(char.IsAsciiDigit))
            return false;

        return value[^1] - '0' == CheckDigit(value.AsSpan(0, value.Length - 1));
    }

    /// <summary>
    /// Kontrol hanesi hariç haneler için GS1 kontrol hanesi: sağdan başlayarak haneler sırayla 3 ve 1 ile çarpılıp
    /// toplanır; sonucu 10'un katına tamamlayan sayıdır.
    /// </summary>
    public static int CheckDigit(ReadOnlySpan<char> digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[digits.Length - 1 - i] - '0';
            sum += i % 2 == 0 ? digit * 3 : digit;
        }

        return (10 - sum % 10) % 10;
    }
}
