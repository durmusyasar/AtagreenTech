using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;

/// <param name="SearchTerm">Ürün adında veya SKU'da (büyük/küçük harf duyarsız) aranan metin.</param>
public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    bool? IsActive = null
) : IQuery<PagedResult<ProductDto>>;
