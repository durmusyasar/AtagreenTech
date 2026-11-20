using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Code).ValidCategoryCode(localizer);
        RuleFor(x => x.Translations).ValidTranslations(localizer);
        RuleForEach(x => x.Translations)
            .NotNull().WithRequiredMessage(localizer, ErrorCodes.Fields.Translations)
            .SetValidator(new CategoryTranslationDtoValidator(localizer));
        RuleFor(x => x.ParentCategoryId).ValidParentCategoryId(localizer);
    }
}
