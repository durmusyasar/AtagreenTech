namespace AtaGreenTech.StockService.Application.DTOs;

/// <param name="Name">İstenen dildeki ad; o dilde çeviri yoksa sistemin varsayılan dilindeki ad.</param>
/// <param name="Symbol">İstenen dildeki sembol; o dilde çeviri yoksa sistemin varsayılan dilindeki sembol.</param>
/// <param name="Translations">Birimin tüm dillerdeki çevirileri.</param>
public sealed record UnitOfMeasureDto(
    Guid Id,
    string Code,
    string Name,
    string Symbol,
    bool IsActive,
    IReadOnlyList<UnitOfMeasureTranslationDto> Translations
);
