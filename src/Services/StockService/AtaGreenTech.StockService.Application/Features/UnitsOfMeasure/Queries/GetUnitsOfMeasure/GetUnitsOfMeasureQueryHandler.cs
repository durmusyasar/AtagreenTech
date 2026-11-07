using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

internal sealed class GetUnitsOfMeasureQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetUnitsOfMeasureQuery, IReadOnlyList<UnitOfMeasureDto>>
{
    public async Task<IReadOnlyList<UnitOfMeasureDto>> Handle(
        GetUnitsOfMeasureQuery request,
        CancellationToken cancellationToken
    )
    {
        var unitsOfMeasure = context.UnitsOfMeasure;

        if (request.IsActive is { } isActive)
            unitsOfMeasure = unitsOfMeasure.Where(u => u.IsActive == isActive);

        var rows = await unitsOfMeasure
            .OrderBy(u => u.Code)
            .Select(u => new
            {
                u.Id,
                u.Code,
                u.IsActive,
                Translations = u.Translations
                    .OrderBy(t => t.LanguageCode)
                    .Select(t => new UnitOfMeasureTranslationDto(t.LanguageCode, t.Name, t.Symbol))
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var defaultLanguage = LanguageCode.Normalize(localization.Value.DefaultLanguage);
        var language = request.Language is null ? defaultLanguage : LanguageCode.Normalize(request.Language);

        return rows
            .Select(u =>
            {
                var display = Resolve(u.Translations, language, defaultLanguage);
                return new UnitOfMeasureDto(
                    u.Id,
                    u.Code,
                    display?.Name ?? u.Code,
                    display?.Symbol ?? u.Code,
                    u.IsActive,
                    u.Translations
                );
            })
            .ToList();
    }

    /// <summary>
    /// İstenen dil, yoksa bölgesiz dil (en-US → en), yoksa varsayılan dil, yoksa ilk çeviri.
    /// </summary>
    private static UnitOfMeasureTranslationDto? Resolve(
        IReadOnlyList<UnitOfMeasureTranslationDto> translations,
        string language,
        string defaultLanguage
    )
    {
        var baseLanguage = language.Split('-')[0];

        return translations.FirstOrDefault(t => t.LanguageCode == language)
            ?? translations.FirstOrDefault(t => t.LanguageCode == baseLanguage)
            ?? translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage)
            ?? translations.FirstOrDefault();
    }
}
