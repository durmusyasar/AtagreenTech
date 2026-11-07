using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    string? Description,
    decimal UnitPrice,
    Guid CategoryId,
    Guid UnitOfMeasureId
) : ICommand<Guid>;
