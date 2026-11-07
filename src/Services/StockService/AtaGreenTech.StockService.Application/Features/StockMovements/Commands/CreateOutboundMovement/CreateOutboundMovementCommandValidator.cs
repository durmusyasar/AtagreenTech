using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateOutboundMovement;

internal sealed class CreateOutboundMovementCommandValidator : AbstractValidator<CreateOutboundMovementCommand>
{
    public CreateOutboundMovementCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty().WithMessage("Depo seçilmelidir.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Ürün seçilmelidir.");
        RuleFor(x => x.Quantity).ValidQuantity();
        RuleFor(x => x.UnitPrice).ValidUnitPrice();
        RuleFor(x => x.DocumentReference).ValidDocumentReference();
        RuleFor(x => x.Description).ValidDescription();
    }
}
