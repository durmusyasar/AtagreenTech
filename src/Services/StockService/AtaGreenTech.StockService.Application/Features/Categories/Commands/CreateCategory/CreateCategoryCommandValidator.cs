using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Code).ValidCategoryCode();
        RuleFor(x => x.Translations).ValidTranslations();
        RuleForEach(x => x.Translations)
            .NotNull().WithMessage("Çeviri bilgisi boş olamaz.")
            .SetValidator(new CategoryTranslationDtoValidator());
        RuleFor(x => x.ParentCategoryId).ValidParentCategoryId();
    }
}
