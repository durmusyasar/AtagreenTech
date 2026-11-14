using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;

internal sealed class GetStockBalancesQueryValidator : AbstractValidator<GetStockBalancesQuery>
{
    public GetStockBalancesQueryValidator()
    {
        RuleFor(x => x.Language).ValidLanguageCode().When(x => x.Language is not null);
    }
}
