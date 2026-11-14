using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ürün kimliği boş olamaz.");
    }
}
