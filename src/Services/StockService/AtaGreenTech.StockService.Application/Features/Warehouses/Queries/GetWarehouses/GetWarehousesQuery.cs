using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Queries.GetWarehouses;

/// <param name="SearchTerm">Depo adında veya kodunda (büyük/küçük harf duyarsız) aranan metin.</param>
public sealed record GetWarehousesQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    string? SearchTerm = null,
    bool? IsActive = null
) : IQuery<PagedResult<WarehouseDto>>, IPagedRequest;
