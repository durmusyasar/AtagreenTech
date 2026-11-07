using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateTransferMovement;

internal sealed class CreateTransferMovementCommandValidator : AbstractValidator<CreateTransferMovementCommand>
{
    public CreateTransferMovementCommandValidator()
    {
        RuleFor(x => x.SourceWarehouseId).NotEmpty().WithMessage("Kaynak depo seçilmelidir.");
        RuleFor(x => x.TargetWarehouseId)
            .NotEmpty().WithMessage("Hedef depo seçilmelidir.")
            .NotEqual(x => x.SourceWarehouseId).WithMessage("Kaynak ve hedef depo aynı olamaz.");
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Ürün seçilmelidir.");
        RuleFor(x => x.Quantity).ValidQuantity();
        RuleFor(x => x.UnitPrice).ValidUnitPrice();
        RuleFor(x => x.DocumentReference).ValidDocumentReference();
        RuleFor(x => x.Description).ValidDescription();
    }
}
