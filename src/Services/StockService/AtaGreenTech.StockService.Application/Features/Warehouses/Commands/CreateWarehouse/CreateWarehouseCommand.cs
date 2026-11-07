using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.CreateWarehouse;

public sealed record CreateWarehouseCommand(string Name, string Code, string? Address) : ICommand<Guid>;
