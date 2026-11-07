using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.UpdateWarehouse;

internal sealed class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Depo kimliği boş olamaz.");
        RuleFor(x => x.Name).ValidWarehouseName();
        RuleFor(x => x.Code).ValidWarehouseCode();
        RuleFor(x => x.Address).ValidWarehouseAddress();
    }
}
