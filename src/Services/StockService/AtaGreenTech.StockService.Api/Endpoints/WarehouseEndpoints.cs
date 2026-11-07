using AtaGreenTech.StockService.Application.Features.Warehouses.Commands.CreateWarehouse;
using AtaGreenTech.StockService.Application.Features.Warehouses.Commands.UpdateWarehouse;
using AtaGreenTech.StockService.Application.Features.Warehouses.Queries.GetWarehouses;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/warehouses</c>: depo tanımları.
/// </summary>
public sealed class WarehouseEndpoints : IEndpointModule
{
    public string Name => "warehouses";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            string? searchTerm = null,
            bool? isActive = null
        ) => sender.Send(new GetWarehousesQuery(pageNumber, pageSize, searchTerm, isActive), cancellationToken));

        group.MapPost("", async (CreateWarehouseCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return TypedResults.Created((string?)null, id);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateWarehouseRequest request,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            await sender.Send(
                new UpdateWarehouseCommand(id, request.Name, request.Code, request.Address, request.IsActive),
                cancellationToken
            );
            return TypedResults.NoContent();
        });
    }
}

public sealed record UpdateWarehouseRequest(string Name, string Code, string? Address, bool IsActive);
