using AtaGreenTech.StockService.Domain.Localization;
using AtaGreenTech.StockService.Domain.Pricing;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Katalogdaki ürün. Dilden bağımsız SKU ve barkod ürünü tanımlar; adı ve açıklaması dillere göre
/// <see cref="Translations"/>'da tutulur.
/// </summary>
public class Product : BaseEntity
{
    public const int SkuMaxLength = 64;
    public const string SkuPattern = "^[A-Za-z0-9._-]+$";

    private readonly List<ProductTranslation> _translations = [];

    protected Product()
    {
        Sku = null!;
        Currency = null!;
        Category = null!;
        UnitOfMeasure = null!;
    }

    /// <param name="defaultLanguage">Çeviriler arasında bulunması zorunlu olan sistem dili.</param>
    public Product(
        string sku,
        string? barcode,
        decimal unitPrice,
        string currency,
        Guid categoryId,
        Guid unitOfMeasureId,
        IReadOnlyCollection<ProductTranslation> translations,
        string defaultLanguage
    )
        : this()
    {
        SetDetails(sku, barcode, unitPrice, currency, categoryId, unitOfMeasureId);
        SetTranslations(translations, defaultLanguage);
    }

    /// <summary>
    /// Silinmemiş ürünler arasında benzersiz stok kodu; büyük harfe çevrilerek saklanır.
    /// </summary>
    public string Sku { get; private set; }

    /// <summary>
    /// GS1 barkodu (EAN-8, UPC-A, EAN-13, GTIN-14); verilmişse silinmemiş ürünler arasında benzersizdir.
    /// </summary>
    public string? Barcode { get; private set; }

    /// <summary>
    /// <see cref="Currency"/> cinsinden birim maliyet; stok hareketlerinde varsayılan birim fiyat olarak önerilir.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Ürünün ve stok hareketlerindeki birim fiyatlarının ISO 4217 para birimi (ör. <c>TRY</c>).
    /// </summary>
    public string Currency { get; private set; }

    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; }

    public Guid UnitOfMeasureId { get; private set; }

    public UnitOfMeasure UnitOfMeasure { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<ProductTranslation> Translations => _translations.AsReadOnly();

    /// <summary>
    /// Ürün bilgilerini ve çevirilerini günceller. Var olan dillerin metni yerinde güncellenir, yeni diller eklenir,
    /// listede olmayan diller silinir.
    /// </summary>
    /// <remarks>
    /// Stok hareketi olan ürünün para biriminin değiştirilmemesi hareketlerin okunmasını gerektirdiği için application
    /// katmanında kontrol edilir.
    /// </remarks>
    public void Update(
        string sku,
        string? barcode,
        decimal unitPrice,
        string currency,
        Guid categoryId,
        Guid unitOfMeasureId,
        IReadOnlyCollection<ProductTranslation> translations,
        string defaultLanguage,
        bool isActive
    )
    {
        // Çeviri kuralları ihlal edilirse ürün hiç değişmesin diye çeviriler önce güncellenir.
        SetTranslations(translations, defaultLanguage);
        SetDetails(sku, barcode, unitPrice, currency, categoryId, unitOfMeasureId);
        IsActive = isActive;
    }

    public static string NormalizeSku(string sku) => sku.Trim().ToUpperInvariant();

    private void SetDetails(
        string sku,
        string? barcode,
        decimal unitPrice,
        string currency,
        Guid categoryId,
        Guid unitOfMeasureId
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Sku = NormalizeSku(sku);
        Barcode = Gtin.Normalize(barcode);
        UnitPrice = unitPrice;
        Currency = CurrencyCode.Normalize(currency);
        CategoryId = categoryId;
        UnitOfMeasureId = unitOfMeasureId;
    }

    private void SetTranslations(IReadOnlyCollection<ProductTranslation> translations, string defaultLanguage) =>
        TranslationCollection.Replace(
            _translations,
            translations,
            defaultLanguage,
            (existing, updated) => existing.SetText(updated.Name, updated.Description)
        );
}
