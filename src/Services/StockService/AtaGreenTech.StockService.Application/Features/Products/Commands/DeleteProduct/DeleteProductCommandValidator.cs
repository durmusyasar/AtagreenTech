using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.ProductId);
    }
}
