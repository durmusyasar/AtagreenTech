using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
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

        var resolver = TranslationResolver.For(request.Language, localization);

        return rows
            .Select(u =>
            {
                var display = resolver.Resolve(u.Translations);
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
}
