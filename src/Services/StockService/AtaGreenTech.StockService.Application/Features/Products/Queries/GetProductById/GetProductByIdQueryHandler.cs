using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IStockDbContext context) : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        await context.Products
            .Where(p => p.Id == request.Id)
            .Select(ProductProjections.ToDto)
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Product), request.Id);
}
