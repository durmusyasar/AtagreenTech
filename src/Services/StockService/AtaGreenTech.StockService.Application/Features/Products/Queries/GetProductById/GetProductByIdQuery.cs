using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
