using AtaGreenTech.StockService.Domain.Localization;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Ürünlerin stokta tutulduğu ölçü birimi. Dilden bağımsız kodu (ör. KG, ADET, LT) ürün ve stok kayıtlarında
/// birimi tanımlar; adı ve sembolü dillere göre <see cref="Translations"/>'da tutulur.
/// </summary>
public class UnitOfMeasure : BaseEntity
{
    public const int CodeMaxLength = 16;

    private readonly List<UnitOfMeasureTranslation> _translations = [];

    protected UnitOfMeasure()
    {
        Code = null!;
    }

    /// <param name="defaultLanguage">Çeviriler arasında bulunması zorunlu olan sistem dili.</param>
    public UnitOfMeasure(string code, IReadOnlyCollection<UnitOfMeasureTranslation> translations, string defaultLanguage)
        : this()
    {
        SetCode(code);
        SetTranslations(translations, defaultLanguage);
    }

    /// <summary>
    /// Silinmemiş birimler arasında benzersiz kısa kod; büyük harfe çevrilerek saklanır.
    /// </summary>
    public string Code { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<UnitOfMeasureTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Kodu, aktifliği ve çevirileri günceller. Var olan dillerin metni yerinde güncellenir, yeni diller eklenir,
    /// listede olmayan diller silinir.
    /// </summary>
    public void Update(
        string code,
        IReadOnlyCollection<UnitOfMeasureTranslation> translations,
        string defaultLanguage,
        bool isActive
    )
    {
        SetCode(code);
        SetTranslations(translations, defaultLanguage);
        IsActive = isActive;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private void SetCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = NormalizeCode(code);
    }

    private void SetTranslations(IReadOnlyCollection<UnitOfMeasureTranslation> translations, string defaultLanguage)
    {
        ArgumentNullException.ThrowIfNull(translations);
        TranslationSetGuard.Ensure(translations.Select(t => t.LanguageCode).ToList(), defaultLanguage);

        _translations.RemoveAll(existing => translations.All(t => t.LanguageCode != existing.LanguageCode));

        foreach (var translation in translations)
        {
            var existing = _translations.Find(t => t.LanguageCode == translation.LanguageCode);
            if (existing is null)
                _translations.Add(translation);
            else
                existing.SetText(translation.Name, translation.Symbol);
        }
    }
}
