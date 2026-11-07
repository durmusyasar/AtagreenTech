using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateOutboundMovement;

/// <param name="DocumentReference">Fatura, irsaliye veya sevk belgesi numarası.</param>
public sealed record CreateOutboundMovementCommand(
    Guid WarehouseId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    string? DocumentReference,
    string? Description
) : ICommand<Guid>;
