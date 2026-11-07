using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.StockMovements;

/// <summary>
/// Stok hareketine konu olan depo ve ürünün var, silinmemiş ve aktif olduğunu doğrular (422).
/// </summary>
internal static class MovementPreconditions
{
    public static async Task EnsureUsableAsync(
        this IReadOnlyRepository<Warehouse> warehouses,
        Guid warehouseId,
        CancellationToken cancellationToken
    )
    {
        var warehouse = await warehouses.GetByIdAsync(warehouseId, cancellationToken);

        CheckRule.ThrowIf(new ReferenceMustExistRule(warehouse is not null), InventoryErrors.WarehouseNotFound, warehouseId);
        CheckRule.ThrowIf(new EntityMustBeActiveRule(warehouse!.IsActive), InventoryErrors.WarehouseInactive, warehouseId);
    }

    public static async Task EnsureUsableAsync(
        this IReadOnlyRepository<Product> products,
        Guid productId,
        CancellationToken cancellationToken
    )
    {
        var product = await products.GetByIdAsync(productId, cancellationToken);

        CheckRule.ThrowIf(new ReferenceMustExistRule(product is not null), InventoryErrors.ProductNotFound, productId);
        CheckRule.ThrowIf(new EntityMustBeActiveRule(product!.IsActive), InventoryErrors.ProductInactive, productId);
    }
}
