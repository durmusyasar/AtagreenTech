using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;

/// <param name="Language">Ad, açıklama ve kategori adının gösterileceği dil; verilmezse sistemin varsayılan dili.</param>
public sealed record GetProductByIdQuery(Guid Id, string? Language = null) : IQuery<ProductDto>;
