using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Language).ValidLanguageCode(localizer).When(x => x.Language is not null);
    }
}
