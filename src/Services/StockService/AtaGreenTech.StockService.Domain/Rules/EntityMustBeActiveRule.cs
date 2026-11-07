using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

/// <summary>
/// Stok hareketine konu olan depo veya ürünün pasife alınmamış olması.
/// </summary>
public sealed class EntityMustBeActiveRule(bool isActive) : IBusinessRule
{
    public bool IsBroken() => !isActive;
}
