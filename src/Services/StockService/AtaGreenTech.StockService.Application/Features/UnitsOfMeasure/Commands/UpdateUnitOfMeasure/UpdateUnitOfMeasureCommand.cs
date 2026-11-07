using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.UpdateUnitOfMeasure;

/// <param name="Translations">
/// Birimin güncel çeviri seti; listede olmayan diller silinir. En az iki farklı dil ve sistemin varsayılan dili
/// bulunmalıdır.
/// </param>
public sealed record UpdateUnitOfMeasureCommand(
    Guid Id,
    string Code,
    IReadOnlyList<UnitOfMeasureTranslationDto> Translations,
    bool IsActive
) : ICommand;
