using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.UpdateWarehouse;

internal sealed class UpdateWarehouseCommandHandler(IRepository<Warehouse> warehouses)
    : ICommandHandler<UpdateWarehouseCommand>
{
    public async Task Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouses.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Warehouse), request.Id);

        var code = Warehouse.NormalizeCode(request.Code);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(
                await warehouses.ExistsAsync(w => w.Code == code && w.Id != request.Id, cancellationToken)
            ),
            InventoryErrors.WarehouseCodeAlreadyExists,
            code
        );

        // Entity repository'den takip edilerek okunduğu için değişiklikleri command'ın transaction'ı kaydeder.
        warehouse.Update(request.Name, code, request.Address, request.IsActive);
    }
}
