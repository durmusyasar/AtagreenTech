using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;

internal sealed class GetStockBalancesQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetStockBalancesQuery, PagedResult<StockBalanceDto>>
{
    public async Task<PagedResult<StockBalanceDto>> Handle(
        GetStockBalancesQuery request,
        CancellationToken cancellationToken
    )
    {
        var resolver = TranslationResolver.For(request.Language, localization);
        var balances = context.StockBalances;

        if (request.WarehouseId is { } warehouseId)
            balances = balances.Where(b => b.WarehouseId == warehouseId);

        if (request.ProductId is { } productId)
            balances = balances.Where(b => b.ProductId == productId);

        if (!request.IncludeEmpty)
            balances = balances.Where(b => b.Quantity != 0);

        var page = await balances
            .OrderBy(b => b.Warehouse.Code)
            .ThenBy(b => b.Product.Sku)
            .Select(b => new
            {
                b.WarehouseId,
                WarehouseCode = b.Warehouse.Code,
                WarehouseName = b.Warehouse.Name,
                b.ProductId,
                b.Product.Sku,
                ProductTranslations = b.Product.Translations
                    .Select(t => new ProductTranslationDto(t.LanguageCode, t.Name, t.Description))
                    .ToList(),
                UnitOfMeasureCode = b.Product.UnitOfMeasure.Code,
                b.Quantity,
                LastChangedAt = b.UpdatedAt ?? b.CreatedAt,
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return page.Map(b => new StockBalanceDto(
            b.WarehouseId,
            b.WarehouseCode,
            b.WarehouseName,
            b.ProductId,
            b.Sku,
            resolver.Resolve(b.ProductTranslations)?.Name ?? b.Sku,
            b.UnitOfMeasureCode,
            b.Quantity,
            b.LastChangedAt
        ));
    }
}
