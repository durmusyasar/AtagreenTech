using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Queries.GetWarehouses;

internal sealed class GetWarehousesQueryHandler(IStockDbContext context)
    : IQueryHandler<GetWarehousesQuery, PagedResult<WarehouseDto>>
{
    public Task<PagedResult<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = context.Warehouses;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var lowerTerm = term.ToLowerInvariant();
            var upperTerm = term.ToUpperInvariant();

            // Kod büyük harfle saklandığı için doğrudan, ad ise küçük harfe çevrilerek karşılaştırılır.
            warehouses = warehouses.Where(w => w.Name.ToLower().Contains(lowerTerm) || w.Code.Contains(upperTerm));
        }

        if (request.IsActive is { } isActive)
            warehouses = warehouses.Where(w => w.IsActive == isActive);

        return warehouses
            .OrderBy(w => w.Code)
            .ThenBy(w => w.Id)
            .Select(w => new WarehouseDto(w.Id, w.Name, w.Code, w.Address, w.IsActive, w.CreatedAt))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
