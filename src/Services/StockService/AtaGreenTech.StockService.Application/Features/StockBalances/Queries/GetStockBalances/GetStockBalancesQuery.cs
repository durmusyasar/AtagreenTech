using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;

/// <param name="IncludeEmpty">Miktarı sıfıra inmiş bakiyeler de listelensin mi.</param>
public sealed record GetStockBalancesQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    Guid? WarehouseId = null,
    Guid? ProductId = null,
    bool IncludeEmpty = false
) : IQuery<PagedResult<StockBalanceDto>>;
