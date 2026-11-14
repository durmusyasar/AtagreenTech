using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Language).ValidLanguageCode().When(x => x.Language is not null);
    }
}
