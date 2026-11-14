using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Features.Products;

internal static class ProductTranslationMapping
{
    public static IReadOnlyCollection<ProductTranslation> ToEntities(this IEnumerable<ProductTranslationDto> translations) =>
        translations.Select(t => new ProductTranslation(t.LanguageCode, t.Name, t.Description)).ToList();
}
