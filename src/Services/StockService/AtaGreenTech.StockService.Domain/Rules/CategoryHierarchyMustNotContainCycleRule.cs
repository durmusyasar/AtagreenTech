using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Rules;

/// <summary>
/// Yeni üst kategorinin, güncellenen kategorinin kendi alt kategorilerinden biri olmaması (A → B → A).
/// </summary>
public sealed class CategoryHierarchyMustNotContainCycleRule(bool createsCycle) : IBusinessRule
{
    public bool IsBroken() => createsCycle;
}
