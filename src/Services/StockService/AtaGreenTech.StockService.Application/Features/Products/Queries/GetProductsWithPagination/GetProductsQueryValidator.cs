using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Language).ValidLanguageCode(localizer).When(x => x.Language is not null);
    }
}
