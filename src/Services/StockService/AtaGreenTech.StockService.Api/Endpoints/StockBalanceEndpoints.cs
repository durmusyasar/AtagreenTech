using AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/stock-balances</c>: depo bazında anlık stok miktarları. Bakiyeler yalnızca stok hareketleriyle değişir.
/// </summary>
public sealed class StockBalanceEndpoints : IEndpointModule
{
    public string Name => "stock-balances";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            Guid? warehouseId = null,
            Guid? productId = null,
            bool includeEmpty = false,
            string? language = null
        ) => sender.Send(
            new GetStockBalancesQuery(pageNumber, pageSize, warehouseId, productId, includeEmpty, language),
            cancellationToken
        ));
    }
}
