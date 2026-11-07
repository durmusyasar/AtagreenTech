using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Bir ürünün bir depodaki anlık miktarı. Her (depo, ürün) için tek kayıt tutulur; böylece bakiye geçmiş hareketler
/// toplanmadan okunur ve eşzamanlı hareketler bu satır üzerinde sıraya sokulabilir.
/// </summary>
/// <remarks>
/// Bakiyeyi değiştiren kod satırı önce kilitlemelidir (application katmanındaki <c>IStockBalanceLocker</c>); kilidi
/// atlayan bir güncelleme veritabanı satır sürümü (<c>xmin</c>) üzerinden eşzamanlılık hatasıyla reddedilir.
/// </remarks>
public class StockBalance : BaseEntity
{
    /// <summary>
    /// Miktarların saklandığı ondalık basamak sayısı (ör. 12,5 kg veya 0,25 lt).
    /// </summary>
    public const int QuantityScale = 4;

    protected StockBalance()
    {
        Warehouse = null!;
        Product = null!;
    }

    public StockBalance(Guid warehouseId, Guid productId)
        : this()
    {
        WarehouseId = warehouseId;
        ProductId = productId;
    }

    public Guid WarehouseId { get; private set; }

    public Warehouse Warehouse { get; private set; }

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; }

    public decimal Quantity { get; private set; }

    public void Increase(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        Quantity += amount;
    }

    public void Decrease(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        CheckRule.ThrowIf(
            new StockMustBeSufficientRule(Quantity, amount),
            InventoryErrors.InsufficientStock,
            WarehouseId,
            ProductId,
            Quantity,
            amount
        );

        Quantity -= amount;
    }
}
