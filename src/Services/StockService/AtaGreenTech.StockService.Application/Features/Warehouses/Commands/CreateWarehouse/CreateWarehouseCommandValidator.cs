using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.CreateWarehouse;

internal sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).ValidWarehouseName();
        RuleFor(x => x.Code).ValidWarehouseCode();
        RuleFor(x => x.Address).ValidWarehouseAddress();
    }
}
