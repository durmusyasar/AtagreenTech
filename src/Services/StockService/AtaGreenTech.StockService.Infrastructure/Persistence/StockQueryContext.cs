using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.StockService.Infrastructure.Persistence;

/// <summary>
/// <see cref="IStockDbContext"/>'i isteğin tenant'ına bağlı <see cref="TenantDbContext"/> üzerinden sağlar. Ayrı bir
/// DbContext türetilmez: tenant bağlantı dizesi yalnızca Core'un kaydettiği <see cref="TenantDbContext"/>'e atanır.
/// </summary>
internal sealed class StockQueryContext(TenantDbContext context) : IStockDbContext
{
    public IQueryable<Product> Products => context.Set<Product>().AsNoTracking();

    public IQueryable<Category> Categories => context.Set<Category>().AsNoTracking();

    public IQueryable<UnitOfMeasure> UnitsOfMeasure => context.Set<UnitOfMeasure>().AsNoTracking();
}
