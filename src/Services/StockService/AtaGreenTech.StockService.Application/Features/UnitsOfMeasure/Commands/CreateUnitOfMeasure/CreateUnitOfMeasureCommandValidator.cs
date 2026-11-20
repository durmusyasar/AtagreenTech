using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;

internal sealed class CreateUnitOfMeasureCommandValidator : AbstractValidator<CreateUnitOfMeasureCommand>
{
    public CreateUnitOfMeasureCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Code).ValidUnitOfMeasureCode(localizer);
        RuleFor(x => x.Translations).ValidTranslations(localizer);
        RuleForEach(x => x.Translations)
            .NotNull().WithRequiredMessage(localizer, ErrorCodes.Fields.Translations)
            .SetValidator(new UnitOfMeasureTranslationDtoValidator(localizer));
    }
}
