using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;

internal sealed class GetStockMovementsQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    public async Task<PagedResult<StockMovementDto>> Handle(
        GetStockMovementsQuery request,
        CancellationToken cancellationToken
    )
    {
        var resolver = TranslationResolver.For(request.Language, localization);
        var movements = context.StockMovements;

        if (request.ProductId is { } productId)
            movements = movements.Where(m => m.ProductId == productId);

        if (request.WarehouseId is { } warehouseId)
            movements = movements.Where(m => m.SourceWarehouseId == warehouseId || m.TargetWarehouseId == warehouseId);

        if (request.Type is { } type)
            movements = movements.Where(m => m.Type == type);

        // Npgsql timestamptz parametrelerini yalnızca UTC kabul eder.
        if (request.From is { } from)
        {
            var fromUtc = from.UtcDateTime;
            movements = movements.Where(m => m.CreatedAt >= fromUtc);
        }

        if (request.To is { } to)
        {
            var toUtc = to.UtcDateTime;
            movements = movements.Where(m => m.CreatedAt < toUtc);
        }

        var page = await movements
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Select(m => new
            {
                m.Id,
                m.Type,
                m.ProductId,
                m.Product.Sku,
                ProductTranslations = m.Product.Translations
                    .Select(t => new ProductTranslationDto(t.LanguageCode, t.Name, t.Description))
                    .ToList(),
                m.Product.Currency,
                m.SourceWarehouseId,
                SourceWarehouseCode = m.SourceWarehouse == null ? null : m.SourceWarehouse.Code,
                m.TargetWarehouseId,
                TargetWarehouseCode = m.TargetWarehouse == null ? null : m.TargetWarehouse.Code,
                m.Quantity,
                m.UnitPrice,
                m.DocumentReference,
                m.Description,
                m.CreatedBy,
                m.CreatedAt,
                m.ClientIpAddress,
                m.CorrelationId,
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return page.Map(m => new StockMovementDto(
            m.Id,
            m.Type,
            m.ProductId,
            m.Sku,
            resolver.Resolve(m.ProductTranslations)?.Name ?? m.Sku,
            m.SourceWarehouseId,
            m.SourceWarehouseCode,
            m.TargetWarehouseId,
            m.TargetWarehouseCode,
            m.Quantity,
            m.UnitPrice,
            m.Currency,
            m.DocumentReference,
            m.Description,
            m.CreatedBy,
            m.CreatedAt,
            m.ClientIpAddress,
            m.CorrelationId
        ));
    }
}
