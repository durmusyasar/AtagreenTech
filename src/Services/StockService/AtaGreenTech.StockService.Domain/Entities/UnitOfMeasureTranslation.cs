using AtaGreenTech.StockService.Domain.Localization;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Ölçü biriminin bir dildeki adı ve sembolü (ör. tr: Adet / ad, en: Piece / pcs).
/// </summary>
public class UnitOfMeasureTranslation : BaseEntity, ITranslation
{
    public const int NameMaxLength = 100;
    public const int SymbolMaxLength = 20;

    protected UnitOfMeasureTranslation()
    {
        LanguageCode = null!;
        Name = null!;
        Symbol = null!;
    }

    public UnitOfMeasureTranslation(string languageCode, string name, string symbol)
        : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        LanguageCode = Localization.LanguageCode.Normalize(languageCode);
        SetText(name, symbol);
    }

    public Guid UnitOfMeasureId { get; private set; }

    /// <summary>
    /// Normalize edilmiş dil kodu (ör. <c>tr</c>, <c>en-US</c>); bir birimde her dil en fazla bir kez bulunur.
    /// </summary>
    public string LanguageCode { get; private set; }

    public string Name { get; private set; }

    public string Symbol { get; private set; }

    internal void SetText(string name, string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        Name = name.Trim();
        Symbol = symbol.Trim();
    }
}
