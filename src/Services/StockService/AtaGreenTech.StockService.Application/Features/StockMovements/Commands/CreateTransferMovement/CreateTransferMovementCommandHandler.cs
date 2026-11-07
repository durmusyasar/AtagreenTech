using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateTransferMovement;

internal sealed class CreateTransferMovementCommandHandler(
    IReadOnlyRepository<Warehouse> warehouses,
    IReadOnlyRepository<Product> products,
    IRepository<StockMovement> movements,
    IStockBalanceLocker balanceLocker,
    IRequestOriginProvider requestOrigin
) : ICommandHandler<CreateTransferMovementCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransferMovementCommand request, CancellationToken cancellationToken)
    {
        var movement = StockMovement.Transfer(
            request.ProductId,
            request.SourceWarehouseId,
            request.TargetWarehouseId,
            request.Quantity,
            request.UnitPrice,
            request.DocumentReference,
            request.Description,
            requestOrigin.GetOrigin()
        );

        await warehouses.EnsureUsableAsync(request.SourceWarehouseId, cancellationToken);
        await warehouses.EnsureUsableAsync(request.TargetWarehouseId, cancellationToken);
        await products.EnsureUsableAsync(request.ProductId, cancellationToken);

        var balances = await balanceLocker.AcquireAsync(
            request.ProductId,
            [request.SourceWarehouseId, request.TargetWarehouseId],
            cancellationToken
        );
        balances[request.SourceWarehouseId].Decrease(request.Quantity);
        balances[request.TargetWarehouseId].Increase(request.Quantity);

        await movements.AddAsync(movement, cancellationToken);

        return movement.Id;
    }
}
