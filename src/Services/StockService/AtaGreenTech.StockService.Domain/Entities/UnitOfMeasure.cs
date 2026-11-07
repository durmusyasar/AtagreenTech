using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Ürünlerin stokta tutulduğu ölçü birimi (ör. ADET / Adet, KG / Kilogram, M / Metre, LT / Litre, PKT / Paket).
/// </summary>
public class UnitOfMeasure : BaseEntity
{
    public const int CodeMaxLength = 16;
    public const int NameMaxLength = 50;

    protected UnitOfMeasure()
    {
        Code = null!;
        Name = null!;
    }

    public UnitOfMeasure(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = NormalizeCode(code);
        Name = name.Trim();
    }

    /// <summary>
    /// Silinmemiş birimler arasında benzersiz kısa kod; büyük harfe çevrilerek saklanır.
    /// </summary>
    public string Code { get; private set; }

    public string Name { get; private set; }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
}
