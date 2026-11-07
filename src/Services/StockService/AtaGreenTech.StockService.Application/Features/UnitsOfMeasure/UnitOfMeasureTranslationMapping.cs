using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure;

internal static class UnitOfMeasureTranslationMapping
{
    public static IReadOnlyCollection<UnitOfMeasureTranslation> ToEntities(
        this IEnumerable<UnitOfMeasureTranslationDto> translations
    ) => translations.Select(t => new UnitOfMeasureTranslation(t.LanguageCode, t.Name, t.Symbol)).ToList();
}
