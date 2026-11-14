using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;

/// <param name="Barcode">İsteğe bağlı GS1 barkodu (EAN-8, UPC-A, EAN-13, GTIN-14).</param>
/// <param name="UnitPrice"><paramref name="Currency"/> cinsinden birim maliyet.</param>
/// <param name="Currency">ISO 4217 para birimi (TRY, USD, EUR).</param>
/// <param name="Translations">En az iki dil; sistemin varsayılan dili (<c>Localization:DefaultLanguage</c>) zorunlu.</param>
public sealed record CreateProductCommand(
    string Sku,
    string? Barcode,
    decimal UnitPrice,
    string Currency,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    IReadOnlyList<ProductTranslationDto> Translations
) : ICommand<Guid>;
