using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Localization;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;

internal sealed class GetProductsQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = context.Products;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var lowerTerm = term.ToLowerInvariant();
            var upperTerm = term.ToUpperInvariant();

            // SKU büyük harfle saklandığı için doğrudan, ad ise küçük harfe çevrilerek karşılaştırılır.
            products = products.Where(p => p.Name.ToLower().Contains(lowerTerm) || p.Sku.Contains(upperTerm));
        }

        if (request.CategoryId is { } categoryId)
            products = products.Where(p => p.CategoryId == categoryId);

        if (request.IsActive is { } isActive)
            products = products.Where(p => p.IsActive == isActive);

        return products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Select(ProductProjections.ToDto(LanguageCode.Normalize(localization.Value.DefaultLanguage)))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
