using AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;
using AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;
using AtaGreenTech.StockService.Application.Features.StockBalances.Queries.GetStockBalances;
using AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;
using AtaGreenTech.StockService.Application.Features.Warehouses.Queries.GetWarehouses;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Tests.Application;

/// <summary>
/// Sayfa numarası ve boyutu OmniSync'in doğrulama davranışında denetlenir; bunun için sorgular
/// <see cref="IPagedRequest"/> uygulamalıdır.
/// </summary>
public class PagedQueryTests
{
    public static TheoryData<object> PagedQueries =>
    [
        new GetCategoriesQuery(),
        new GetProductsQuery(),
        new GetStockBalancesQuery(),
        new GetStockMovementsQuery(),
        new GetWarehousesQuery(),
    ];

    [Theory]
    [MemberData(nameof(PagedQueries))]
    public void Paged_queries_are_validated_by_core(object query)
    {
        Assert.IsAssignableFrom<IPagedRequest>(query);
    }
}
