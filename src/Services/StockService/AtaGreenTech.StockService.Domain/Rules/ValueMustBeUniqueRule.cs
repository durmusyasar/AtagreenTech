using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

/// <summary>
/// Benzersiz olması gereken bir değerin (SKU, birim kodu) silinmemiş başka bir kayıtta kullanılmaması.
/// </summary>
public sealed class ValueMustBeUniqueRule(bool valueAlreadyExists) : IBusinessRule
{
    public bool IsBroken() => valueAlreadyExists;
}
