namespace AtaGreenTech.StockService.Application.DTOs;

public sealed record WarehouseDto(
    Guid Id,
    string Name,
    string Code,
    string? Address,
    bool IsActive,
    DateTime CreatedAt
);
