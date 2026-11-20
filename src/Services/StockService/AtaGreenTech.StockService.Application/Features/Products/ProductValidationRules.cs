using AtaGreenTech.SharedKernel.Localization;
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

    public static IRuleBuilderOptions<T, string> ValidSku<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Sku)
            .MaximumLength(Product.SkuMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Sku, Product.SkuMaxLength)
            .Matches(Product.SkuPattern)
            .WithLocalizedMessage(localizer, ErrorCodes.ValidationSkuPattern);

    public static IRuleBuilderOptions<T, string?> ValidBarcode<T>(
        this IRuleBuilder<T, string?> rule,
        IErrorLocalizer localizer
    ) =>
        rule.Must(barcode => string.IsNullOrWhiteSpace(barcode) || Gtin.IsValid(barcode))
            .WithLocalizedMessage(localizer, ErrorCodes.ValidationGtin);

    public static IRuleBuilderOptions<T, decimal> ValidUnitCost<T>(
        this IRuleBuilder<T, decimal> rule,
        IErrorLocalizer localizer
    ) =>
        rule.GreaterThan(0)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationGreaterThanZero,
                localizer.Localize(ErrorCodes.Fields.UnitCost)
            )
            .PrecisionScale(PricePrecision, PriceScale, ignoreTrailingZeros: true)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationPrecision,
                localizer.Localize(ErrorCodes.Fields.UnitCost),
                PriceScale
            );

    public static IRuleBuilderOptions<T, string> ValidCurrency<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Currency)
            .Must(CurrencyCode.IsSupported)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationCurrency,
                string.Join(", ", CurrencyCode.Supported)
            );
}

internal sealed class ProductTranslationDtoValidator : AbstractValidator<ProductTranslationDto>
{
    public ProductTranslationDtoValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Language)
            .ValidLanguageCode(localizer);

        RuleFor(x => x.Name)
            .NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Name)
            .MaximumLength(ProductTranslation.NameMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Name, ProductTranslation.NameMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(ProductTranslation.DescriptionMaxLength)
            .WithMaxLengthMessage(
                localizer,
                ErrorCodes.Fields.Description,
                ProductTranslation.DescriptionMaxLength
            );
    }
}
