using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Features.Categories;

internal static class CategoryTranslationMapping
{
    public static IReadOnlyCollection<CategoryTranslation> ToEntities(this IEnumerable<CategoryTranslationDto> translations) =>
        translations.Select(t => new CategoryTranslation(t.LanguageCode, t.Name, t.Description)).ToList();
}
