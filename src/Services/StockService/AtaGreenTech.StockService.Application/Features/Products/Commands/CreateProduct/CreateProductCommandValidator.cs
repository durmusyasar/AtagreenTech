using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(Product.NameMaxLength).WithMessage($"Ürün adı en fazla {Product.NameMaxLength} karakter olabilir.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU boş olamaz.")
            .MaximumLength(Product.SkuMaxLength).WithMessage($"SKU en fazla {Product.SkuMaxLength} karakter olabilir.")
            .Matches(Product.SkuPattern).WithMessage("SKU yalnızca harf, rakam, nokta, tire ve alt çizgi içerebilir.");

        RuleFor(x => x.Description)
            .MaximumLength(Product.DescriptionMaxLength)
            .WithMessage($"Açıklama en fazla {Product.DescriptionMaxLength} karakter olabilir.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Birim fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçilmelidir.");

        RuleFor(x => x.UnitOfMeasureId)
            .NotEmpty().WithMessage("Ölçü birimi seçilmelidir.");
    }
}
