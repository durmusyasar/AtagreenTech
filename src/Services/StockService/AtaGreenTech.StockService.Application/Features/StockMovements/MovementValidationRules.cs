using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements;

internal static class MovementValidationRules
{
    private const int AmountPrecision = 18;
    private const int PriceScale = 2;

    public static IRuleBuilderOptions<T, decimal> ValidQuantity<T>(
        this IRuleBuilder<T, decimal> rule,
        IErrorLocalizer localizer
    ) =>
        rule.GreaterThan(0)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationGreaterThanZero,
                localizer.Localize(ErrorCodes.Fields.Quantity)
            )
            .PrecisionScale(AmountPrecision, StockBalance.QuantityScale, ignoreTrailingZeros: true)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationPrecision,
                localizer.Localize(ErrorCodes.Fields.Quantity),
                StockBalance.QuantityScale
            );

    public static IRuleBuilderOptions<T, decimal> ValidUnitPrice<T>(
        this IRuleBuilder<T, decimal> rule,
        IErrorLocalizer localizer
    ) =>
        rule.GreaterThanOrEqualTo(0)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationGreaterOrEqualZero,
                localizer.Localize(ErrorCodes.Fields.UnitPrice)
            )
            .PrecisionScale(AmountPrecision, PriceScale, ignoreTrailingZeros: true)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationPrecision,
                localizer.Localize(ErrorCodes.Fields.UnitPrice),
                PriceScale
            );

    public static IRuleBuilderOptions<T, string?> ValidDocumentReference<T>(
        this IRuleBuilder<T, string?> rule,
        IErrorLocalizer localizer
    ) =>
        rule.MaximumLength(StockMovement.DocumentReferenceMaxLength)
            .WithMaxLengthMessage(
                localizer,
                ErrorCodes.Fields.Document,
                StockMovement.DocumentReferenceMaxLength
            );

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(
        this IRuleBuilder<T, string?> rule,
        IErrorLocalizer localizer
    ) =>
        rule.MaximumLength(StockMovement.DescriptionMaxLength)
            .WithMaxLengthMessage(
                localizer,
                ErrorCodes.Fields.Description,
                StockMovement.DescriptionMaxLength
            );
}
