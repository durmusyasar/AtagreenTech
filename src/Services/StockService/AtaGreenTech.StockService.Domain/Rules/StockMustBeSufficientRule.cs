using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

internal sealed class StockMustBeSufficientRule(decimal available, decimal requested) : IBusinessRule
{
    public bool IsBroken() => requested > available;
}
