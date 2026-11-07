using System.Linq.Expressions;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Features.Products;

internal static class ProductProjections
{
    /// <param name="defaultLanguage">Kategori adının gösterileceği normalize edilmiş sistem dili.</param>
    public static Expression<Func<Product, ProductDto>> ToDto(string defaultLanguage) => p => new ProductDto(
        p.Id,
        p.Name,
        p.Sku,
        p.Description,
        p.UnitPrice,
        p.CategoryId,
        p.Category.Translations
            .Where(t => t.LanguageCode == defaultLanguage)
            .Select(t => t.Name)
            .FirstOrDefault() ?? p.Category.Code,
        p.UnitOfMeasureId,
        p.UnitOfMeasure.Code,
        p.IsActive,
        p.CreatedAt
    );
}
