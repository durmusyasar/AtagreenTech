using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Interfaces;

/// <summary>
/// Query handler'larının okuma modeli: isteğin tenant veritabanındaki tablolar, değişiklik takibi olmadan
/// (<c>AsNoTracking</c>) ve soft delete filtresi uygulanmış olarak. Yazma işlemleri <c>IRepository&lt;T&gt;</c>
/// üzerinden yapılır; command'ların transaction'ı ve kaydı OmniSync'in pipeline'ı tarafından yönetilir.
/// </summary>
public interface IStockDbContext
{
    IQueryable<Product> Products { get; }

    IQueryable<Category> Categories { get; }

    IQueryable<UnitOfMeasure> UnitsOfMeasure { get; }

    IQueryable<Warehouse> Warehouses { get; }

    IQueryable<StockBalance> StockBalances { get; }

    IQueryable<StockMovement> StockMovements { get; }
}
