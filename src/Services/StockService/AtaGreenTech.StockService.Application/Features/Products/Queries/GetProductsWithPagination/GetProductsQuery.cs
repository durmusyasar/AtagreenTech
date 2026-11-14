using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;

/// <param name="SearchTerm">
/// SKU'da, barkodda veya herhangi bir dildeki ürün adında (büyük/küçük harf duyarsız) aranan metin.
/// </param>
/// <param name="Language">Ad, açıklama ve kategori adının gösterileceği dil; verilmezse sistemin varsayılan dili.</param>
public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    string? SearchTerm = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    string? Language = null
) : IQuery<PagedResult<ProductDto>>, IPagedRequest;
