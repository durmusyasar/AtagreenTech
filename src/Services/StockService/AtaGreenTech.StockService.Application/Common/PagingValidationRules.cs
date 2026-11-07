using FluentValidation;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Common;

internal static class PagingValidationRules
{
    public static IRuleBuilderOptions<T, int> ValidPageNumber<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1 veya daha büyük olmalıdır.");

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(1, BaseSearchModel.MaxPageSize)
            .WithMessage($"Sayfa boyutu 1 ile {BaseSearchModel.MaxPageSize} arasında olmalıdır.");
}
