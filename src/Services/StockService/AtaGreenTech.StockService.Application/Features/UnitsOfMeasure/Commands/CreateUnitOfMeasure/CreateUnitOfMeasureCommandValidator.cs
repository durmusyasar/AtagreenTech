using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;

internal sealed class CreateUnitOfMeasureCommandValidator : AbstractValidator<CreateUnitOfMeasureCommand>
{
    public CreateUnitOfMeasureCommandValidator()
    {
        RuleFor(x => x.Code).ValidUnitOfMeasureCode();
        RuleFor(x => x.Translations).ValidTranslations();
        RuleForEach(x => x.Translations)
            .NotNull().WithMessage("Çeviri bilgisi boş olamaz.")
            .SetValidator(new UnitOfMeasureTranslationDtoValidator());
    }
}
