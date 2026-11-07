using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateTransferMovement;

/// <param name="DocumentReference">Transfer fişi veya sevk irsaliyesi numarası.</param>
public sealed record CreateTransferMovementCommand(
    Guid SourceWarehouseId,
    Guid TargetWarehouseId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    string? DocumentReference,
    string? Description
) : ICommand<Guid>;
