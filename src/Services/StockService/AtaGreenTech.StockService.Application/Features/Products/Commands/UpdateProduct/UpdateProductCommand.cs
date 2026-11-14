using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;

/// <param name="Currency">ISO 4217 para birimi; stok hareketi olan üründe değiştirilemez.</param>
/// <param name="Translations">
/// Ürünün tüm çeviri seti: listede olmayan diller silinir. En az iki dil; sistemin varsayılan dili zorunlu.
/// </param>
public sealed record UpdateProductCommand(
    Guid Id,
    string Sku,
    string? Barcode,
    decimal UnitPrice,
    string Currency,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    IReadOnlyList<ProductTranslationDto> Translations,
    bool IsActive
) : ICommand;
