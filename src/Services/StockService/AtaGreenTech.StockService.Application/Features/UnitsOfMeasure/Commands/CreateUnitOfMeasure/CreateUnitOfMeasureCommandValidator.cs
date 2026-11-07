using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;

internal sealed class CreateUnitOfMeasureCommandValidator : AbstractValidator<CreateUnitOfMeasureCommand>
{
    public CreateUnitOfMeasureCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Birim kodu boş olamaz.")
            .MaximumLength(UnitOfMeasure.CodeMaxLength)
            .WithMessage($"Birim kodu en fazla {UnitOfMeasure.CodeMaxLength} karakter olabilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Birim adı boş olamaz.")
            .MaximumLength(UnitOfMeasure.NameMaxLength)
            .WithMessage($"Birim adı en fazla {UnitOfMeasure.NameMaxLength} karakter olabilir.");
    }
}
