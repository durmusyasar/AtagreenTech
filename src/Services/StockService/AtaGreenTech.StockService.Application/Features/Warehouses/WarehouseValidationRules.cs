using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses;

internal static class WarehouseValidationRules
{
    private const string CodePattern = "^[A-Za-z0-9._-]+$";

    public static IRuleBuilderOptions<T, string> ValidWarehouseName<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Name)
            .MaximumLength(Warehouse.NameMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Name, Warehouse.NameMaxLength);

    public static IRuleBuilderOptions<T, string> ValidWarehouseCode<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.Code)
            .MaximumLength(Warehouse.CodeMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Code, Warehouse.CodeMaxLength)
            .Matches(CodePattern)
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.ValidationCodePattern,
                localizer.Localize(ErrorCodes.Fields.Code)
            );

    public static IRuleBuilderOptions<T, string?> ValidWarehouseAddress<T>(
        this IRuleBuilder<T, string?> rule,
        IErrorLocalizer localizer
    ) =>
        rule.MaximumLength(Warehouse.AddressMaxLength)
            .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Address, Warehouse.AddressMaxLength);
}
