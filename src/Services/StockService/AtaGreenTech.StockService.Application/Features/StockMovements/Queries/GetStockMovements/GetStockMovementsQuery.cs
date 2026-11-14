using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;

/// <param name="WarehouseId">Bu depodan çıkan veya bu depoya giren hareketler.</param>
/// <param name="From">Bu andan (dahil) sonra yapılan hareketler.</param>
/// <param name="To">Bu andan (hariç) önce yapılan hareketler.</param>
public sealed record GetStockMovementsQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    MovementType? Type = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null
) : IQuery<PagedResult<StockMovementDto>>, IPagedRequest;
