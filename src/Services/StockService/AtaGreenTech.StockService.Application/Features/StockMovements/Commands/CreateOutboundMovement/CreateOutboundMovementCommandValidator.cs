using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateOutboundMovement;

internal sealed class CreateOutboundMovementCommandValidator : AbstractValidator<CreateOutboundMovementCommand>
{
    public CreateOutboundMovementCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.WarehouseId).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Warehouse);
        RuleFor(x => x.ProductId).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Product);
        RuleFor(x => x.Quantity).ValidQuantity(localizer);
        RuleFor(x => x.UnitPrice).ValidUnitPrice(localizer);
        RuleFor(x => x.DocumentReference).ValidDocumentReference(localizer);
        RuleFor(x => x.Description).ValidDescription(localizer);
    }
}
