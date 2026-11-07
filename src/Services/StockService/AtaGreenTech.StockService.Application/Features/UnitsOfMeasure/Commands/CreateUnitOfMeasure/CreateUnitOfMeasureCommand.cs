using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;

/// <param name="Code">Dilden bağımsız benzersiz birim kodu (ör. KG).</param>
/// <param name="Translations">En az iki farklı dilde, sistemin varsayılan dilini içeren ad ve sembol çevirileri.</param>
public sealed record CreateUnitOfMeasureCommand(string Code, IReadOnlyList<UnitOfMeasureTranslationDto> Translations)
    : ICommand<Guid>;
