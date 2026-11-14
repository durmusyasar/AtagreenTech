using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Products;

/// <summary>
/// Ürün kaydı ve güncellemesinin ortak iş kuralları (422).
/// </summary>
internal static class ProductPreconditions
{
    /// <summary>
    /// Normalize edilmiş SKU'nun ve barkodun silinmemiş başka bir üründe kullanılmadığını doğrular.
    /// </summary>
    /// <param name="productId">Güncellenen ürün; yeni ürün için <see langword="null"/>.</param>
    public static async Task EnsureUniqueAsync(
        this IReadOnlyRepository<Product> products,
        string sku,
        string? barcode,
        Guid? productId,
        CancellationToken cancellationToken
    )
    {
        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(
                await products.ExistsAsync(p => p.Sku == sku && p.Id != productId, cancellationToken)
            ),
            CatalogErrors.SkuAlreadyExists,
            sku
        );

        if (barcode is not null)
        {
            CheckRule.ThrowIf(
                new ValueMustBeUniqueRule(
                    await products.ExistsAsync(p => p.Barcode == barcode && p.Id != productId, cancellationToken)
                ),
                CatalogErrors.BarcodeAlreadyExists,
                barcode
            );
        }
    }

    public static async Task EnsureReferencesExistAsync(
        this IReadOnlyRepository<Category> categories,
        IReadOnlyRepository<UnitOfMeasure> unitsOfMeasure,
        Guid categoryId,
        Guid unitOfMeasureId,
        CancellationToken cancellationToken
    )
    {
        CheckRule.ThrowIf(
            new ReferenceMustExistRule(await categories.ExistsAsync(c => c.Id == categoryId, cancellationToken)),
            CatalogErrors.CategoryNotFound,
            categoryId
        );
        CheckRule.ThrowIf(
            new ReferenceMustExistRule(await unitsOfMeasure.ExistsAsync(u => u.Id == unitOfMeasureId, cancellationToken)),
            CatalogErrors.UnitOfMeasureNotFound,
            unitOfMeasureId
        );
    }
}
