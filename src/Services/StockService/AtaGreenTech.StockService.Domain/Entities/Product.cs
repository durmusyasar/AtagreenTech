using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

public class Product : BaseEntity
{
    public const int NameMaxLength = 200;
    public const int SkuMaxLength = 64;
    public const int DescriptionMaxLength = 1000;
    public const string SkuPattern = "^[A-Za-z0-9._-]+$";

    protected Product()
    {
        Name = null!;
        Sku = null!;
        Category = null!;
        UnitOfMeasure = null!;
    }

    public Product(string name, string sku, string? description, decimal unitPrice, Guid categoryId, Guid unitOfMeasureId)
        : this()
    {
        SetDetails(name, sku, description, unitPrice, categoryId, unitOfMeasureId);
    }

    public string Name { get; private set; }

    /// <summary>
    /// Silinmemiş ürünler arasında benzersiz stok kodu / barkod; büyük harfe çevrilerek saklanır.
    /// </summary>
    public string Sku { get; private set; }

    public string? Description { get; private set; }

    public decimal UnitPrice { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; }

    public Guid UnitOfMeasureId { get; private set; }

    public UnitOfMeasure UnitOfMeasure { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void Update(
        string name,
        string sku,
        string? description,
        decimal unitPrice,
        Guid categoryId,
        Guid unitOfMeasureId,
        bool isActive
    )
    {
        SetDetails(name, sku, description, unitPrice, categoryId, unitOfMeasureId);
        IsActive = isActive;
    }

    public static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    private void SetDetails(
        string name,
        string sku,
        string? description,
        decimal unitPrice,
        Guid categoryId,
        Guid unitOfMeasureId
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Name = name.Trim();
        Sku = NormalizeSku(sku);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UnitPrice = unitPrice;
        CategoryId = categoryId;
        UnitOfMeasureId = unitOfMeasureId;
    }
}
