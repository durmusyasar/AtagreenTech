using System.Text.RegularExpressions;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Localization;
using AtaGreenTech.StockService.Infrastructure.Persistence;
using AtaGreenTech.StockService.Infrastructure.Persistence.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OmniSync.Core.Infrastructure.Hosting;

namespace AtaGreenTech.StockService.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Stok servisinin entity eşlemelerini OmniSync'in tenant modeline ekler; okuma modelini, bakiye kilidini ve
    /// master marka okuyucusunu kaydeder. Tablolar her tenant'ın kendi veritabanında Core'un <c>TenantDbContext</c>'i
    /// üzerinden kullanılır; ayrı bir DbContext tanımlanmaz.
    /// </summary>
    public static OmniSyncCoreOptions AddStockInfrastructure(this OmniSyncCoreOptions options)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        options.ConfigureTenantModel(model => model.ApplyConfigurationsFromAssembly(assembly));

        // Tenant modeli özelleştirildiğinde migration'lar runtime modeliyle aynı assembly'den okunmalıdır.
        options.TenantMigrationsAssembly = assembly;

        options.ConfigureServices(
            (services, configuration) =>
            {
                services.AddScoped<IStockDbContext, StockQueryContext>();
                services.AddScoped<IStockBalanceLocker, StockBalanceLocker>();
                services.AddScoped<ITenantBrandingReader, TenantBrandingReader>();

                services.AddAuthorization(options =>
                {
                    foreach (var (policy, permission) in AuthorizationPolicies.Bindings)
                        options.AddPolicy(policy, p => p.RequireClaim(JwtContract.Claims.Permission, permission));
                });

                services.AddOptions<LocalizationOptions>()
                    .Bind(configuration.GetSection(LocalizationOptions.SectionName))
                    .Validate(
                        o => o.DefaultLanguage is not null && Regex.IsMatch(o.DefaultLanguage, LanguageCode.Pattern),
                        "Localization:DefaultLanguage must be a language code such as 'tr' or 'en-US'."
                    )
                    .ValidateOnStart();
            }
        );

        return options;
    }
}
