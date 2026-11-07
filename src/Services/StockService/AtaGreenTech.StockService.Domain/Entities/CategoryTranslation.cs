using AtaGreenTech.StockService.Domain.Localization;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Kategorinin bir dildeki adı ve açıklaması (ör. tr: Tohum, en: Seeds).
/// </summary>
public class CategoryTranslation : BaseEntity, ITranslation
{
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 500;

    protected CategoryTranslation()
    {
        LanguageCode = null!;
        Name = null!;
    }

    public CategoryTranslation(string languageCode, string name, string? description)
        : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(languageCode);

        LanguageCode = Localization.LanguageCode.Normalize(languageCode);
        SetText(name, description);
    }

    public Guid CategoryId { get; private set; }

    /// <summary>
    /// Normalize edilmiş dil kodu (ör. <c>tr</c>, <c>en-US</c>); bir kategoride her dil en fazla bir kez bulunur.
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
