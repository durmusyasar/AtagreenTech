using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateOutboundMovement;

internal sealed class CreateOutboundMovementCommandHandler(
    IReadOnlyRepository<Warehouse> warehouses,
    IReadOnlyRepository<Product> products,
    IRepository<StockMovement> movements,
    IStockBalanceLocker balanceLocker,
    IRequestOriginProvider requestOrigin
) : ICommandHandler<CreateOutboundMovementCommand, Guid>
{
    public async Task<Guid> Handle(CreateOutboundMovementCommand request, CancellationToken cancellationToken)
    {
        await warehouses.EnsureUsableAsync(request.WarehouseId, cancellationToken);
        await products.EnsureUsableAsync(request.ProductId, cancellationToken);

        var balances = await balanceLocker.AcquireAsync(request.ProductId, [request.WarehouseId], cancellationToken);
        balances[request.WarehouseId].Decrease(request.Quantity);

        var movement = await movements.AddAsync(
            StockMovement.Outbound(
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
