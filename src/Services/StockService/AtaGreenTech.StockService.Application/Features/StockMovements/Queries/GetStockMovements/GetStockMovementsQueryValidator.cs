using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;

internal sealed class GetStockMovementsQueryValidator : AbstractValidator<GetStockMovementsQuery>
{
    public GetStockMovementsQueryValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Type).IsInEnum()
            .WithLocalizedMessage(localizer, ErrorCodes.ValidationInvalid, localizer.Localize(ErrorCodes.Fields.MovementType));

        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithLocalizedMessage(localizer, ErrorCodes.ValidationDateRange);

        RuleFor(x => x.Language).ValidLanguageCode(localizer).When(x => x.Language is not null);
    }
}
