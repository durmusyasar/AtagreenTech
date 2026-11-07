using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Categories;

internal static class CategoryValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidCategoryCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Kategori kodu boş olamaz.")
            .MaximumLength(Category.CodeMaxLength)
            .WithMessage($"Kategori kodu en fazla {Category.CodeMaxLength} karakter olabilir.");

    public static IRuleBuilderOptions<T, Guid?> ValidParentCategoryId<T>(this IRuleBuilder<T, Guid?> rule) =>
        rule.NotEqual(Guid.Empty).WithMessage("Üst kategori kimliği geçersiz.");
}

internal sealed class CategoryTranslationDtoValidator : AbstractValidator<CategoryTranslationDto>
{
    public CategoryTranslationDtoValidator()
    {
        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu boş olamaz.")
            .ValidLanguageCode();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı boş olamaz.")
            .MaximumLength(CategoryTranslation.NameMaxLength)
            .WithMessage($"Kategori adı en fazla {CategoryTranslation.NameMaxLength} karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(CategoryTranslation.DescriptionMaxLength)
            .WithMessage($"Açıklama en fazla {CategoryTranslation.DescriptionMaxLength} karakter olabilir.");
    }
}
