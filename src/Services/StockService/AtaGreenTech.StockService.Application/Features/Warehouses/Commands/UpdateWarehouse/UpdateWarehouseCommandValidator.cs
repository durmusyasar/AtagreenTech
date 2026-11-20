using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.UpdateWarehouse;

internal sealed class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.WarehouseId);
        RuleFor(x => x.Name).ValidWarehouseName(localizer);
        RuleFor(x => x.Code).ValidWarehouseCode(localizer);
        RuleFor(x => x.Address).ValidWarehouseAddress(localizer);
    }
}
