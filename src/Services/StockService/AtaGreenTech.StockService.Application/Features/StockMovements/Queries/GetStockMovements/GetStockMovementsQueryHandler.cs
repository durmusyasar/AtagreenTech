using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;

internal sealed class GetStockMovementsQueryHandler(IStockDbContext context)
    : IQueryHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    public Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var movements = context.StockMovements;

        if (request.ProductId is { } productId)
            movements = movements.Where(m => m.ProductId == productId);

        if (request.WarehouseId is { } warehouseId)
            movements = movements.Where(m => m.SourceWarehouseId == warehouseId || m.TargetWarehouseId == warehouseId);

        if (request.Type is { } type)
            movements = movements.Where(m => m.Type == type);

        // Npgsql timestamptz parametrelerini yalnızca UTC kabul eder.
        if (request.From is { } from)
        {
            var fromUtc = from.UtcDateTime;
            movements = movements.Where(m => m.CreatedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtc = to.UtcDateTime;
            movements = movements.Where(m => m.CreatedAt < toUtc);
        }

        return movements
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Select(m => new StockMovementDto(
                m.Id,
                m.Type,
                m.ProductId,
                m.Product.Sku,
                m.Product.Name,
                m.SourceWarehouseId,
                m.SourceWarehouse == null ? null : m.SourceWarehouse.Code,
                m.TargetWarehouseId,
                m.TargetWarehouse == null ? null : m.TargetWarehouse.Code,
                m.Quantity,
                m.UnitPrice,
                m.DocumentReference,
                m.Description,
                m.CreatedBy,
                m.CreatedAt,
                m.ClientIpAddress,
                m.CorrelationId
            ))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
