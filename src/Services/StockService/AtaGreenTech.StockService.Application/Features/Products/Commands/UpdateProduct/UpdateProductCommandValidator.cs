using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ürün kimliği boş olamaz.");

        RuleFor(x => x.Sku).ValidSku();
        RuleFor(x => x.Barcode).ValidBarcode();
        RuleFor(x => x.UnitPrice).ValidUnitCost();
        RuleFor(x => x.Currency).ValidCurrency();

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçilmelidir.");

        RuleFor(x => x.UnitOfMeasureId)
            .NotEmpty().WithMessage("Ölçü birimi seçilmelidir.");

        RuleFor(x => x.Translations).ValidTranslations();
        RuleForEach(x => x.Translations)
            .NotNull().WithMessage("Çeviri bilgisi boş olamaz.")
            .SetValidator(new ProductTranslationDtoValidator());
    }
}
