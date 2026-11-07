using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

internal sealed class GetUnitsOfMeasureQueryValidator : AbstractValidator<GetUnitsOfMeasureQuery>
{
    public GetUnitsOfMeasureQueryValidator()
    {
        RuleFor(x => x.Language).ValidLanguageCode().When(x => x.Language is not null);
    }
}
