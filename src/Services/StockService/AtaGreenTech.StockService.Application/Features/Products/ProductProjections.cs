using System.Linq.Expressions;
using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Features.Products;

internal static class ProductProjections
{
    /// <summary>
    /// Veritabanından okunan ürün satırı; ad ve kategori adı dil seçimi için tüm çevirileriyle gelir.
    /// </summary>
    public sealed record ProductRow(
        Guid Id,
        string Sku,
        string? Barcode,
        decimal UnitPrice,
        string Currency,
        Guid CategoryId,
        string CategoryCode,
        List<CategoryTranslationDto> CategoryTranslations,
        Guid UnitOfMeasureId,
        string UnitOfMeasureCode,
        bool IsActive,
        DateTime CreatedAt,
        List<ProductTranslationDto> Translations
    );

    public static readonly Expression<Func<Product, ProductRow>> ToRow = p => new ProductRow(
        p.Id,
        p.Sku,
        p.Barcode,
        p.UnitPrice,
        p.Currency,
        p.CategoryId,
        p.Category.Code,
        p.Category.Translations.Select(t => new CategoryTranslationDto(t.LanguageCode, t.Name, t.Description)).ToList(),
        p.UnitOfMeasureId,
        p.UnitOfMeasure.Code,
        p.IsActive,
        p.CreatedAt,
        p.Translations
            .OrderBy(t => t.LanguageCode)
            .Select(t => new ProductTranslationDto(t.LanguageCode, t.Name, t.Description))
            .ToList()
    );

    public static ProductDto ToDto(this ProductRow row, TranslationResolver resolver)
    {
        var display = resolver.Resolve(row.Translations);

        return new ProductDto(
            row.Id,
            row.Sku,
            row.Barcode,
            display?.Name ?? row.Sku,
            display?.Description,
            row.UnitPrice,
            row.Currency,
            row.CategoryId,
            resolver.Resolve(row.CategoryTranslations)?.Name ?? row.CategoryCode,
            row.UnitOfMeasureId,
            row.UnitOfMeasureCode,
            row.IsActive,
            row.CreatedAt,
            row.Translations
        );
    }
}
