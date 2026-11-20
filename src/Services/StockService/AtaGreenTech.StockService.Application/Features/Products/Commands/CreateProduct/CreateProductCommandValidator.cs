using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Sku).ValidSku(localizer);
        RuleFor(x => x.Barcode).ValidBarcode(localizer);
        RuleFor(x => x.UnitPrice).ValidUnitCost(localizer);
        RuleFor(x => x.Currency).ValidCurrency(localizer);
        RuleFor(x => x.CategoryId).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Category);
        RuleFor(x => x.UnitOfMeasureId).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Unit);
        RuleFor(x => x.Translations).ValidTranslations(localizer);
        RuleForEach(x => x.Translations)
            .NotNull().WithRequiredMessage(localizer, ErrorCodes.Fields.Translations)
            .SetValidator(new ProductTranslationDtoValidator(localizer));
    }
}
