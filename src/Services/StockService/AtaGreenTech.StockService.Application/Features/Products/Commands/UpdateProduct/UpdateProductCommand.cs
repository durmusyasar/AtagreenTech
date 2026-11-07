using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    string Sku,
    string? Description,
    decimal UnitPrice,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    bool IsActive
) : ICommand;
