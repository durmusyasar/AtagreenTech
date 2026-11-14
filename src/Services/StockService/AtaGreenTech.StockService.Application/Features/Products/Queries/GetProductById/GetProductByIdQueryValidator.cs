using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Language).ValidLanguageCode().When(x => x.Language is not null);
    }
}
