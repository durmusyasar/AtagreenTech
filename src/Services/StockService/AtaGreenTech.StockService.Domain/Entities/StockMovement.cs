using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Değiştirilemez stok hareketi kaydı; bakiyelerin denetim izi. Hareketi yapan kullanıcı ve zaman
/// <see cref="BaseEntity.CreatedBy"/> / <see cref="BaseEntity.CreatedAt"/> alanlarında, tenant ise kaydın tutulduğu
/// tenant veritabanının kendisinde saklıdır.
/// </summary>
public class StockMovement : BaseEntity
{
    public const int DocumentReferenceMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int ClientIpAddressMaxLength = 45;
    public const int CorrelationIdMaxLength = 64;

    protected StockMovement()
    {
        Product = null!;
    }

    private StockMovement(
        MovementType type,
        Guid productId,
        Guid? sourceWarehouseId,
        Guid? targetWarehouseId,
        decimal quantity,
        decimal unitPrice,
        string? documentReference,
        string? description,
        MovementOrigin origin
    )
        : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Type = type;
        ProductId = productId;
        SourceWarehouseId = sourceWarehouseId;
        TargetWarehouseId = targetWarehouseId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DocumentReference = Normalize(documentReference);
        Description = Normalize(description);
        ClientIpAddress = Normalize(origin.ClientIpAddress);
        CorrelationId = Normalize(origin.CorrelationId);
    }

    public MovementType Type { get; private set; }

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; }

    /// <summary>
    /// Stoğun çıktığı depo; girişlerde <see langword="null"/>.
    /// </summary>
    public Guid? SourceWarehouseId { get; private set; }

    public Warehouse? SourceWarehouse { get; private set; }

    /// <summary>
    /// Stoğun girdiği depo; çıkışlarda <see langword="null"/>.
    /// </summary>
    public Guid? TargetWarehouseId { get; private set; }

    public Warehouse? TargetWarehouse { get; private set; }

    public decimal Quantity { get; private set; }

    /// <summary>
    /// Hareket anındaki birim fiyat / maliyet.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Fatura, irsaliye veya üretim emri numarası.
    /// </summary>
    public string? DocumentReference { get; private set; }

    public string? Description { get; private set; }

    /// <summary>
    /// Hareketi başlatan istemcinin IP adresi; HTTP dışı işlemlerde <see langword="null"/>.
    /// </summary>
    public string? ClientIpAddress { get; private set; }

    /// <summary>
    /// Hareketi oluşturan isteğin izleme kimliği (trace id); loglardaki istek kaydıyla eşleştirmek için.
    /// </summary>
    public string? CorrelationId { get; private set; }

    public static StockMovement Inbound(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        decimal unitPrice,
        string? documentReference,
        string? description,
        MovementOrigin origin
    ) =>
        new(MovementType.Inbound, productId, null, warehouseId, quantity, unitPrice, documentReference, description, origin);

    public static StockMovement Outbound(
        Guid productId,
        Guid warehouseId,
        decimal quantity,
        decimal unitPrice,
        string? documentReference,
        string? description,
        MovementOrigin origin
    ) =>
        new(MovementType.Outbound, productId, warehouseId, null, quantity, unitPrice, documentReference, description, origin);

    public static StockMovement Transfer(
        Guid productId,
        Guid sourceWarehouseId,
        Guid targetWarehouseId,
        decimal quantity,
        decimal unitPrice,
        string? documentReference,
        string? description,
        MovementOrigin origin
    )
    {
        CheckRule.ThrowIf(
            new TransferWarehousesMustDifferRule(sourceWarehouseId, targetWarehouseId),
            InventoryErrors.TransferWarehousesMustDiffer,
            sourceWarehouseId
        );

        return new(
            MovementType.Transfer,
            productId,
            sourceWarehouseId,
            targetWarehouseId,
            quantity,
            unitPrice,
            documentReference,
            description,
            origin
        );
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
