namespace AtaGreenTech.StockService.Domain.Errors;

/// <summary>
/// Depo ve stok hareketi iş kuralı ihlallerinin resource key'leri; OmniSync bunları 422 yanıtının
/// <c>resourceKey</c> alanında döndürür.
/// </summary>
public static class InventoryErrors
{
    public const string InsufficientStock = "Inventory.InsufficientStock";
    public const string TransferWarehousesMustDiffer = "Inventory.TransferWarehousesMustDiffer";
    public const string WarehouseCodeAlreadyExists = "Inventory.WarehouseCodeAlreadyExists";
    public const string WarehouseNotFound = "Inventory.WarehouseNotFound";
    public const string WarehouseInactive = "Inventory.WarehouseInactive";
    public const string ProductNotFound = "Inventory.ProductNotFound";
    public const string ProductInactive = "Inventory.ProductInactive";
}
