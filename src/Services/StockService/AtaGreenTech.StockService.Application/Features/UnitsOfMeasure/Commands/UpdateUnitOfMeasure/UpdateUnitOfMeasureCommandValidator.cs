using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.UpdateUnitOfMeasure;

internal sealed class UpdateUnitOfMeasureCommandValidator : AbstractValidator<UpdateUnitOfMeasureCommand>
{
    public UpdateUnitOfMeasureCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.UnitId);
        RuleFor(x => x.Code).ValidUnitOfMeasureCode(localizer);
        RuleFor(x => x.Translations).ValidTranslations(localizer);
        RuleForEach(x => x.Translations)
            .NotNull().WithRequiredMessage(localizer, ErrorCodes.Fields.Translations)
            .SetValidator(new UnitOfMeasureTranslationDtoValidator(localizer));
    }
}
