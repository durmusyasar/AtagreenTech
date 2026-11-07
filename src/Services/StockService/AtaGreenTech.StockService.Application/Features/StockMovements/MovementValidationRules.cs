using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements;

/// <summary>
/// Giriş, çıkış ve transfer komutlarının ortak alan kuralları.
/// </summary>
internal static class MovementValidationRules
{
    private const int AmountPrecision = 18;
    private const int PriceScale = 2;

    public static IRuleBuilderOptions<T, decimal> ValidQuantity<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.")
            .PrecisionScale(AmountPrecision, StockBalance.QuantityScale, ignoreTrailingZeros: true)
            .WithMessage($"Miktar en fazla {StockBalance.QuantityScale} ondalık basamak içerebilir.");

    public static IRuleBuilderOptions<T, decimal> ValidUnitPrice<T>(this IRuleBuilder<T, decimal> rule) =>
        rule.GreaterThanOrEqualTo(0).WithMessage("Birim fiyat 0'dan küçük olamaz.")
            .PrecisionScale(AmountPrecision, PriceScale, ignoreTrailingZeros: true)
            .WithMessage($"Birim fiyat en fazla {PriceScale} ondalık basamak içerebilir.");

    public static IRuleBuilderOptions<T, string?> ValidDocumentReference<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(StockMovement.DocumentReferenceMaxLength)
            .WithMessage($"Belge numarası en fazla {StockMovement.DocumentReferenceMaxLength} karakter olabilir.");

    public static IRuleBuilderOptions<T, string?> ValidDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(StockMovement.DescriptionMaxLength)
            .WithMessage($"Açıklama en fazla {StockMovement.DescriptionMaxLength} karakter olabilir.");
}
