using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Commands.UpdateWarehouse;

public sealed record UpdateWarehouseCommand(Guid Id, string Name, string Code, string? Address, bool IsActive) : ICommand;
