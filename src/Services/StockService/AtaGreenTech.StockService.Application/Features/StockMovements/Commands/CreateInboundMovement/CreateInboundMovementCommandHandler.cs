using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateInboundMovement;

internal sealed class CreateInboundMovementCommandHandler(
    IReadOnlyRepository<Warehouse> warehouses,
    IReadOnlyRepository<Product> products,
    IRepository<StockMovement> movements,
    IStockBalanceLocker balanceLocker,
    IRequestOriginProvider requestOrigin
) : ICommandHandler<CreateInboundMovementCommand, Guid>
{
    public async Task<Guid> Handle(CreateInboundMovementCommand request, CancellationToken cancellationToken)
    {
        await warehouses.EnsureUsableAsync(request.WarehouseId, cancellationToken);
        await products.EnsureUsableAsync(request.ProductId, cancellationToken);

        var balances = await balanceLocker.AcquireAsync(request.ProductId, [request.WarehouseId], cancellationToken);
        balances[request.WarehouseId].Increase(request.Quantity);

        var movement = await movements.AddAsync(
            StockMovement.Inbound(
                request.ProductId,
                request.WarehouseId,
                request.Quantity,
                request.UnitPrice,
                request.DocumentReference,
                request.Description,
                requestOrigin.GetOrigin()
            ),
            cancellationToken
        );

        return movement.Id;
    }
}
