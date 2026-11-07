using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken) =>
        await context.Products
            .Where(p => p.Id == request.Id)
            .Select(ProductProjections.ToDto(LanguageCode.Normalize(localization.Value.DefaultLanguage)))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(nameof(Product), request.Id);
}
