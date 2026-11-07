using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

/// <summary>
/// Bir kaydın referans verdiği kaydın (kategori, ölçü birimi, üst kategori) var ve silinmemiş olması.
/// </summary>
public sealed class ReferenceMustExistRule(bool referenceExists) : IBusinessRule
{
    public bool IsBroken() => !referenceExists;
}
