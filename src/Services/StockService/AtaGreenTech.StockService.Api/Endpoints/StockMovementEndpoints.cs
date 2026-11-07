using AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateInboundMovement;
using AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateOutboundMovement;
using AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateTransferMovement;
using AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;
using AtaGreenTech.StockService.Domain.Entities;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/stock-movements</c>: stok giriş, çıkış ve transferleri ile hareket geçmişi.
/// </summary>
public sealed class StockMovementEndpoints : IEndpointModule
{
    public string Name => "stock-movements";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            Guid? productId = null,
            Guid? warehouseId = null,
            MovementType? type = null,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null
        ) => sender.Send(
            new GetStockMovementsQuery(pageNumber, pageSize, productId, warehouseId, type, from, to),
            cancellationToken
        ));

        group.MapPost("/inbound", async (
            CreateInboundMovementCommand command,
            ISender sender,
            CancellationToken cancellationToken
        ) => TypedResults.Created((string?)null, await sender.Send(command, cancellationToken)));

        group.MapPost("/outbound", async (
            CreateOutboundMovementCommand command,
            ISender sender,
            CancellationToken cancellationToken
        ) => TypedResults.Created((string?)null, await sender.Send(command, cancellationToken)));

        group.MapPost("/transfer", async (
            CreateTransferMovementCommand command,
            ISender sender,
            CancellationToken cancellationToken
        ) => TypedResults.Created((string?)null, await sender.Send(command, cancellationToken)));
    }
}
