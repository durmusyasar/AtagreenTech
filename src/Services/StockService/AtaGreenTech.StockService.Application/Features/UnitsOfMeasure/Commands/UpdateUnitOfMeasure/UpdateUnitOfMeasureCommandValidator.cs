using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.UpdateUnitOfMeasure;

internal sealed class UpdateUnitOfMeasureCommandValidator : AbstractValidator<UpdateUnitOfMeasureCommand>
{
    public UpdateUnitOfMeasureCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Birim kimliği boş olamaz.");
        RuleFor(x => x.Code).ValidUnitOfMeasureCode();
        RuleFor(x => x.Translations).ValidTranslations();
        RuleForEach(x => x.Translations)
            .NotNull().WithMessage("Çeviri bilgisi boş olamaz.")
            .SetValidator(new UnitOfMeasureTranslationDtoValidator());
    }
}
