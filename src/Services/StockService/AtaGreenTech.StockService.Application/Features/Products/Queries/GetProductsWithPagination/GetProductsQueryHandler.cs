using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;

internal sealed class GetProductsQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var resolver = TranslationResolver.For(request.Language, localization);
        var defaultLanguage = resolver.DefaultLanguage;
        var products = context.Products;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var lowerTerm = term.ToLowerInvariant();
            var upperTerm = term.ToUpperInvariant();

            // SKU büyük harfle saklandığı için doğrudan, adlar ise küçük harfe çevrilerek karşılaştırılır.
            products = products.Where(p =>
                p.Sku.Contains(upperTerm)
                || (p.Barcode != null && p.Barcode.Contains(term))
                || p.Translations.Any(t => t.Name.ToLower().Contains(lowerTerm))
            );
        }

        if (request.CategoryId is { } categoryId)
            products = products.Where(p => p.CategoryId == categoryId);

        if (request.IsActive is { } isActive)
            products = products.Where(p => p.IsActive == isActive);

        var page = await products
            .OrderBy(p => p.Translations.Where(t => t.LanguageCode == defaultLanguage).Select(t => t.Name).FirstOrDefault())
            .ThenBy(p => p.Id)
            .Select(ProductProjections.ToRow)
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return page.Map(row => row.ToDto(resolver));
    }
}
