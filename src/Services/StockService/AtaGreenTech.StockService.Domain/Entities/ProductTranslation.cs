using AtaGreenTech.StockService.Domain.Localization;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Ürünün bir dildeki adı ve açıklaması (ör. tr: Damla Sulama Borusu, en: Drip Irrigation Pipe).
/// </summary>
public class ProductTranslation : BaseEntity, ITranslation
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 1000;

    protected ProductTranslation()
    {
        LanguageCode = null!;
        Name = null!;
    }

    public ProductTranslation(string languageCode, string name, string? description)
        : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        LanguageCode = Localization.LanguageCode.Normalize(languageCode);
        SetText(name, description);
    }

    public Guid ProductId { get; private set; }

    /// <summary>
    /// Normalize edilmiş dil kodu (ör. <c>tr</c>, <c>en-US</c>); bir üründe her dil en fazla bir kez bulunur.
    /// </summary>
    public string LanguageCode { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    internal void SetText(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
