using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

internal sealed class GetUnitsOfMeasureQueryValidator : AbstractValidator<GetUnitsOfMeasureQuery>
{
    public GetUnitsOfMeasureQueryValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Language).ValidLanguageCode(localizer).When(x => x.Language is not null);
    }
}
