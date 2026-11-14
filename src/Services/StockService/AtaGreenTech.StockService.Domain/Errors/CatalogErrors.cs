namespace AtaGreenTech.StockService.Domain.Errors;

/// <summary>
/// İş kuralı ihlallerinin resource key'leri; OmniSync bunları 422 yanıtının <c>resourceKey</c> alanında döndürür.
/// </summary>
public static class CatalogErrors
{
    public const string CategoryCannotBeItsOwnParent = "Catalog.CategoryCannotBeItsOwnParent";
    public const string CategoryHierarchyCycle = "Catalog.CategoryHierarchyCycle";
    public const string CategoryNotFound = "Catalog.CategoryNotFound";
    public const string CategoryCodeAlreadyExists = "Catalog.CategoryCodeAlreadyExists";
    public const string ParentCategoryNotFound = "Catalog.ParentCategoryNotFound";
    public const string UnitOfMeasureNotFound = "Catalog.UnitOfMeasureNotFound";
    public const string SkuAlreadyExists = "Catalog.SkuAlreadyExists";
    public const string BarcodeAlreadyExists = "Catalog.BarcodeAlreadyExists";
    public const string ProductHasStockHistory = "Catalog.ProductHasStockHistory";
    public const string ProductCurrencyLocked = "Catalog.ProductCurrencyLocked";
    public const string UnitOfMeasureCodeAlreadyExists = "Catalog.UnitOfMeasureCodeAlreadyExists";
}
