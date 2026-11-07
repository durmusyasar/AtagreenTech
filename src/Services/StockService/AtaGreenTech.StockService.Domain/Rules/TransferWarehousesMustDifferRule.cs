using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

internal sealed class TransferWarehousesMustDifferRule(Guid sourceWarehouseId, Guid targetWarehouseId) : IBusinessRule
{
    public bool IsBroken() => sourceWarehouseId == targetWarehouseId;
}
