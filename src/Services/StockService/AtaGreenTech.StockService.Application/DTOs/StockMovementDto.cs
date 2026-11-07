using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.DTOs;

/// <param name="CreatedBy">Hareketi yapan kullanıcının kimliği (JWT <c>sub</c>).</param>
public sealed record StockMovementDto(
    Guid Id,
    MovementType Type,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    Guid? SourceWarehouseId,
    string? SourceWarehouseCode,
    Guid? TargetWarehouseId,
    string? TargetWarehouseCode,
    decimal Quantity,
    decimal UnitPrice,
    string? DocumentReference,
    string? Description,
    string? CreatedBy,
    DateTime CreatedAt,
    string? ClientIpAddress,
    string? CorrelationId
);
