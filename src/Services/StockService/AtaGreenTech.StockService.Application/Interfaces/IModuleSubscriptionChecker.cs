namespace AtaGreenTech.StockService.Application.Interfaces;

/// <summary>
/// Tenant ↔ Module abonelik doğrulama (master DB). IsCore kısa yol; aksi halde enabled + unexpired.
/// </summary>
public interface IModuleSubscriptionChecker
{
    Task<bool> IsModuleActiveAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default
    );
}
