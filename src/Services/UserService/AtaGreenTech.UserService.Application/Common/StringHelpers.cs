namespace AtaGreenTech.UserService.Application.Common;

/// <summary>Metin birleştirme / boş değer seçimi yardımcıları.</summary>
public static class StringHelpers
{
    /// <summary>
    /// Boş olmayan ilk değeri trimleyerek döner; hiçbiri dolu değilse <c>null</c>.
    /// İstek + yapılandırma fallback zincirleri için kullanılır.
    /// </summary>
    public static string? FirstNonEmpty(params string?[] values) =>
        FirstNonEmpty((IEnumerable<string?>)values);

    /// <summary>
    /// Dizideki boş olmayan ilk değeri trimleyerek döner; hiçbiri dolu değilse <c>null</c>.
    /// </summary>
    public static string? FirstNonEmpty(IEnumerable<string?> values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }
}
