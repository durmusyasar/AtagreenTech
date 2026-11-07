using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı boş olamaz.")
            .MaximumLength(Category.NameMaxLength)
            .WithMessage($"Kategori adı en fazla {Category.NameMaxLength} karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(Category.DescriptionMaxLength)
            .WithMessage($"Açıklama en fazla {Category.DescriptionMaxLength} karakter olabilir.");

        RuleFor(x => x.ParentCategoryId)
            .NotEqual(Guid.Empty).WithMessage("Üst kategori kimliği geçersiz.");
    }
}
