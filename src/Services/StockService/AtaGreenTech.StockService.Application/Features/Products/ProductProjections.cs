using System.Linq.Expressions;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Features.Products;

internal static class ProductProjections
{
    public static readonly Expression<Func<Product, ProductDto>> ToDto = p => new ProductDto(
        p.Id,
        p.Name,
        p.Sku,
        p.Description,
        p.UnitPrice,
        p.CategoryId,
        p.Category.Name,
        p.UnitOfMeasureId,
        p.UnitOfMeasure.Code,
        p.IsActive,
        p.CreatedAt
    );
}
