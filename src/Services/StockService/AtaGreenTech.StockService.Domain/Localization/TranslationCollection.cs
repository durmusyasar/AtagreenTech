namespace AtaGreenTech.StockService.Domain.Localization;

public static class TranslationCollection
{
    /// <summary>
    /// <paramref name="current"/>'i <paramref name="incoming"/> setine eşitler: önce <see cref="TranslationSetGuard"/>
    /// kuralları uygulanır, sonra listede olmayan diller çıkarılır, var olan dillerin metni yerinde güncellenir ve
    /// yeni diller eklenir. Kurallar ihlal edilirse <paramref name="current"/> değişmez.
    /// </summary>
    /// <param name="copyText">Var olan çeviriye (ilk parametre) yeni metni (ikinci parametre) aktarır.</param>
    public static void Replace<T>(
        List<T> current,
        IReadOnlyCollection<T> incoming,
        string defaultLanguage,
        Action<T, T> copyText
    )
        where T : ITranslation
    {
        ArgumentNullException.ThrowIfNull(incoming);
        TranslationSetGuard.Ensure(incoming.Select(t => t.LanguageCode).ToList(), defaultLanguage);

        current.RemoveAll(existing => incoming.All(t => t.LanguageCode != existing.LanguageCode));

        foreach (var translation in incoming)
        {
            var existing = current.Find(t => t.LanguageCode == translation.LanguageCode);
            if (existing is null)
                current.Add(translation);
            else
                copyText(existing, translation);
        }
    }
}
