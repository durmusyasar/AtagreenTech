using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithRequiredMessage(localizer, ErrorCodes.Fields.CategoryId);
        RuleFor(x => x.Code).ValidCategoryCode(localizer);
        RuleFor(x => x.Translations).ValidTranslations(localizer);
        RuleForEach(x => x.Translations)
            .NotNull().WithRequiredMessage(localizer, ErrorCodes.Fields.Translations)
            .SetValidator(new CategoryTranslationDtoValidator(localizer));
        RuleFor(x => x.ParentCategoryId).ValidParentCategoryId(localizer);
    }
}
