using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;

public sealed record CreateUnitOfMeasureCommand(string Code, string Name) : ICommand<Guid>;
