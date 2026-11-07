using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Domain.Users;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.StockService.Infrastructure.Persistence;

/// <summary>
/// Bakiyeleri PostgreSQL satır kilidiyle (<c>SELECT ... FOR UPDATE</c>) kilitler. Kilit, command pipeline'ının açtığı
/// transaction commit veya rollback olana kadar tutulur.
/// </summary>
internal sealed class StockBalanceLocker(TenantDbContext context, ICurrentUser currentUser) : IStockBalanceLocker
{
    public async Task<IReadOnlyDictionary<Guid, StockBalance>> AcquireAsync(
        Guid productId,
        IReadOnlyCollection<Guid> warehouseIds,
        CancellationToken cancellationToken
    )
    {
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Stok bakiyeleri yalnızca açık bir transaction içinde kilitlenebilir.");

        var balances = new Dictionary<Guid, StockBalance>(warehouseIds.Count);

        foreach (var warehouseId in warehouseIds.Distinct().Order())
        {
            // Eşzamanlı ilk girişlerde ikinci istek, birincinin commit'ini bekler ve yeni kayıt açmadan devam eder.
            await context.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO "StockBalances" ("Id", "WarehouseId", "ProductId", "Quantity", "CreatedAt", "CreatedBy", "IsDeleted")
                VALUES ({Guid.NewGuid()}, {warehouseId}, {productId}, 0, {DateTime.UtcNow}, {currentUser.UserId}, false)
                ON CONFLICT ("WarehouseId", "ProductId") WHERE "IsDeleted" = false DO NOTHING
                """,
                cancellationToken
            );

            balances[warehouseId] = await context.Set<StockBalance>()
                .FromSql(
                    $"""
                    SELECT *, xmin FROM "StockBalances"
                    WHERE "WarehouseId" = {warehouseId} AND "ProductId" = {productId} AND "IsDeleted" = false
                    FOR UPDATE
                    """
                )
                .SingleAsync(cancellationToken);
        }

        return balances;
    }
}
