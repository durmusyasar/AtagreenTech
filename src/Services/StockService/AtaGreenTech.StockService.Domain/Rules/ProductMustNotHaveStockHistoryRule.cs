using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

/// <summary>
/// Stok hareketi görmüş bir ürünün silinmemesi ve para biriminin değiştirilmemesi: hareketlerin birim fiyatları
/// ürünün para biriminde tutulur ve bakiyeler hareketlere dayanır.
/// </summary>
public sealed class ProductMustNotHaveStockHistoryRule(bool hasStockHistory) : IBusinessRule
{
    public bool IsBroken() => hasStockHistory;
}
