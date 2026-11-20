using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.CreateWarehouse;

internal sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Name).ValidWarehouseName(localizer);
        RuleFor(x => x.Code).ValidWarehouseCode(localizer);
        RuleFor(x => x.Address).ValidWarehouseAddress(localizer);
    }
}
