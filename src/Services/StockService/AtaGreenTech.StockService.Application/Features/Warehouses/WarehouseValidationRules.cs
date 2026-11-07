using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses;

internal static class WarehouseValidationRules
{
    /// <summary>
    /// Harf, rakam, nokta, tire ve alt çizgi (ör. MTR-01).
    /// </summary>
    private const string CodePattern = "^[A-Za-z0-9._-]+$";

    public static IRuleBuilderOptions<T, string> ValidWarehouseName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Depo adı boş olamaz.")
            .MaximumLength(Warehouse.NameMaxLength)
            .WithMessage($"Depo adı en fazla {Warehouse.NameMaxLength} karakter olabilir.");

    public static IRuleBuilderOptions<T, string> ValidWarehouseCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Depo kodu boş olamaz.")
            .MaximumLength(Warehouse.CodeMaxLength)
            .WithMessage($"Depo kodu en fazla {Warehouse.CodeMaxLength} karakter olabilir.")
            .Matches(CodePattern).WithMessage("Depo kodu yalnızca harf, rakam, nokta, tire ve alt çizgi içerebilir.");

    public static IRuleBuilderOptions<T, string?> ValidWarehouseAddress<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(Warehouse.AddressMaxLength)
            .WithMessage($"Adres en fazla {Warehouse.AddressMaxLength} karakter olabilir.");
}
