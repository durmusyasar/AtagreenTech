using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Infrastructure.Persistence;
using AtaGreenTech.UserService.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using OmniSync.Core.Infrastructure.Hosting;

namespace AtaGreenTech.UserService.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Kullanıcı servisinin entity eşlemelerini OmniSync tenant modeline ekler. Tablolar her tenant'ın kendi
    /// veritabanında Core'un <c>TenantDbContext</c>'i üzerinden kullanılır; ayrı bir DbContext tanımlanmaz.
    /// </summary>
    public static OmniSyncCoreOptions AddUserInfrastructure(this OmniSyncCoreOptions options)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        options.ConfigureTenantModel(model => model.ApplyConfigurationsFromAssembly(assembly));

        // Tenant modeli özelleştirildiğinde migration'lar runtime modeliyle aynı assembly'den okunmalıdır.
        options.TenantMigrationsAssembly = assembly;

        options.ConfigureServices(
            (services, _) =>
            {
                services.AddScoped<IUserDbContext, UserQueryContext>();
                services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
            }
        );

        return options;
    }
}
