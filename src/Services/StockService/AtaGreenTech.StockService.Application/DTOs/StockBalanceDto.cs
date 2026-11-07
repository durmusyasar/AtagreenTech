namespace AtaGreenTech.StockService.Application.DTOs;

/// <param name="LastChangedAt">Bakiyeyi değiştiren son hareketin zamanı.</param>
public sealed record StockBalanceDto(
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string UnitOfMeasureCode,
    decimal Quantity,
    DateTime LastChangedAt
);
