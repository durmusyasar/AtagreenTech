namespace AtaGreenTech.StockService.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal UnitPrice,
    Guid CategoryId,
    string CategoryName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureCode,
    bool IsActive,
    DateTime CreatedAt
);
