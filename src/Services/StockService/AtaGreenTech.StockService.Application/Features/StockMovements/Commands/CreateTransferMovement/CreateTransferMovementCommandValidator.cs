using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateTransferMovement;

internal sealed class CreateTransferMovementCommandValidator : AbstractValidator<CreateTransferMovementCommand>
{
    public CreateTransferMovementCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.SourceWarehouseId).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.SourceWarehouse);
        RuleFor(x => x.TargetWarehouseId)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.TargetWarehouse)
            .NotEqual(x => x.SourceWarehouseId)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationMustDiffer,
                localizer.Localize(ErrorCodes.Fields.SourceWarehouse),
                localizer.Localize(ErrorCodes.Fields.TargetWarehouse)
            );
        RuleFor(x => x.ProductId).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Product);
        RuleFor(x => x.Quantity).ValidQuantity(localizer);
        RuleFor(x => x.UnitPrice).ValidUnitPrice(localizer);
        RuleFor(x => x.DocumentReference).ValidDocumentReference(localizer);
        RuleFor(x => x.Description).ValidDescription(localizer);
    }
}
