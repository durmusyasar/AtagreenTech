using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;

internal sealed class GetStockBalancesQueryValidator : AbstractValidator<GetStockBalancesQuery>
{
    public GetStockBalancesQueryValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Language).ValidLanguageCode(localizer).When(x => x.Language is not null);
    }
}
