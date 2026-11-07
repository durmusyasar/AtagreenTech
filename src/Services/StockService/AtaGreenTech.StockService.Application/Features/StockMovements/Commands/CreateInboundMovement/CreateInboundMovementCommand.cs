using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateInboundMovement;

/// <param name="DocumentReference">Fatura, irsaliye veya üretim emri numarası.</param>
public sealed record CreateInboundMovementCommand(
    Guid WarehouseId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    string? DocumentReference,
    string? Description
) : ICommand<Guid>;
