using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.CreateWarehouse;

internal sealed class CreateWarehouseCommandHandler(IRepository<Warehouse> warehouses)
    : ICommandHandler<CreateWarehouseCommand, Guid>
{
    public async Task<Guid> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var code = Warehouse.NormalizeCode(request.Code);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(await warehouses.ExistsAsync(w => w.Code == code, cancellationToken)),
            InventoryErrors.WarehouseCodeAlreadyExists,
            code
        );

        var warehouse = await warehouses.AddAsync(new Warehouse(request.Name, code, request.Address), cancellationToken);

        return warehouse.Id;
    }
}
