using FluentValidation;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;

internal sealed class GetCategoriesQueryValidator : AbstractValidator<GetCategoriesQuery>
{
    public GetCategoriesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1 veya daha büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, BaseSearchModel.MaxPageSize)
            .WithMessage($"Sayfa boyutu 1 ile {BaseSearchModel.MaxPageSize} arasında olmalıdır.");
    }
}
