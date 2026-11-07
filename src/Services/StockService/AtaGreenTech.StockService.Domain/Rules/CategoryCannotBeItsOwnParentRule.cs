using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

internal sealed class CategoryCannotBeItsOwnParentRule(Guid categoryId, Guid? parentCategoryId) : IBusinessRule
{
    public bool IsBroken() => parentCategoryId == categoryId;
}
