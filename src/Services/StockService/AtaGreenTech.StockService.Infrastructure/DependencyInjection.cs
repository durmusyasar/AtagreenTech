using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniSync.Core.Infrastructure.Hosting;

namespace AtaGreenTech.StockService.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Stok servisinin entity eşlemelerini OmniSync'in tenant modeline ekler ve okuma modelini kaydeder. Tablolar her
    /// tenant'ın kendi veritabanında Core'un <c>TenantDbContext</c>'i üzerinden kullanılır; ayrı bir DbContext tanımlanmaz.
    /// </summary>
    public static OmniSyncCoreOptions AddStockInfrastructure(this OmniSyncCoreOptions options)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        options.ConfigureTenantModel(model => model.ApplyConfigurationsFromAssembly(assembly));

        // Tenant modeli özelleştirildiğinde migration'lar runtime modeliyle aynı assembly'den okunmalıdır.
        options.TenantMigrationsAssembly = assembly;

        options.ConfigureServices((services, _) => services.AddScoped<IStockDbContext, StockQueryContext>());

        return options;
    }
}
