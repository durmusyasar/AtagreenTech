using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;

internal sealed class GetStockBalancesQueryHandler(IStockDbContext context)
    : IQueryHandler<GetStockBalancesQuery, PagedResult<StockBalanceDto>>
{
    public Task<PagedResult<StockBalanceDto>> Handle(GetStockBalancesQuery request, CancellationToken cancellationToken)
    {
        var balances = context.StockBalances;

        if (request.WarehouseId is { } warehouseId)
            balances = balances.Where(b => b.WarehouseId == warehouseId);

        if (request.ProductId is { } productId)
            balances = balances.Where(b => b.ProductId == productId);

        if (!request.IncludeEmpty)
            balances = balances.Where(b => b.Quantity != 0);

        return balances
            .OrderBy(b => b.Warehouse.Code)
            .ThenBy(b => b.Product.Sku)
            .Select(b => new StockBalanceDto(
                b.WarehouseId,
                b.Warehouse.Code,
                b.Warehouse.Name,
                b.ProductId,
                b.Product.Sku,
                b.Product.Name,
                b.Product.UnitOfMeasure.Code,
                b.Quantity,
                b.UpdatedAt ?? b.CreatedAt
            ))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
