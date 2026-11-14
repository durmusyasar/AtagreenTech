using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Pricing;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products;

internal static class ProductValidationRules
{
    private const int PricePrecision = 18;
    private const int PriceScale = 2;

    public static IRuleBuilderOptions<T, string> ValidSku<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("SKU boş olamaz.")
            .MaximumLength(Product.SkuMaxLength).WithMessage($"SKU en fazla {Product.SkuMaxLength} karakter olabilir.")
            .Matches(Product.SkuPattern).WithMessage("SKU yalnızca harf, rakam, nokta, tire ve alt çizgi içerebilir.");

    /// <summary>
    /// Barkod isteğe bağlıdır; verilirse geçerli bir GS1 barkodu olmalıdır.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidBarcode<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(barcode => string.IsNullOrWhiteSpace(barcode) || Gtin.IsValid(barcode))
            .WithMessage("Barkod 8, 12, 13 veya 14 haneli ve kontrol hanesi doğru bir GS1 barkodu (EAN/UPC) olmalıdır.");

    public static IRuleBuilderOptions<T, decimal> ValidUnitCost<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.GreaterThan(0).WithMessage("Birim maliyet 0'dan büyük olmalıdır.")
            .PrecisionScale(PricePrecision, PriceScale, ignoreTrailingZeros: true)
            .WithMessage($"Birim maliyet en fazla {PriceScale} ondalık basamak içerebilir.");

    public static IRuleBuilderOptions<T, string> ValidCurrency<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Para birimi boş olamaz.")
            .Must(CurrencyCode.IsSupported)
            .WithMessage($"Para birimi şunlardan biri olmalıdır: {string.Join(", ", CurrencyCode.Supported)}.");
}

internal sealed class ProductTranslationDtoValidator : AbstractValidator<ProductTranslationDto>
{
    public ProductTranslationDtoValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu boş olamaz.")
            .ValidLanguageCode();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz.")
            .MaximumLength(ProductTranslation.NameMaxLength)
            .WithMessage($"Ürün adı en fazla {ProductTranslation.NameMaxLength} karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(ProductTranslation.DescriptionMaxLength)
            .WithMessage($"Açıklama en fazla {ProductTranslation.DescriptionMaxLength} karakter olabilir.");
    }
}
