using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace AtaGreenTech.SharedKernel.Security;

public static class AccessTokenSessionAuthenticationExtensions
{
    /// <summary>
    /// Master denylist/stamp store + 403 <c>PERMISSION_DENIED</c> handler.
    /// JwtBearer <c>OnTokenValidated</c> kancası servis Infrastructure'da eklenir.
    /// </summary>
    public static IServiceCollection AddAccessTokenSessionStores<TMasterDbContext>(
        this IServiceCollection services,
        Func<IServiceProvider, IAccessTokenSessionValidator>? validatorFactory = null
    )
        where TMasterDbContext : DbContext
    {
        services.AddMemoryCache();
        services.AddScoped<IAccessTokenRevocationStore>(sp =>
            new MasterAccessTokenRevocationStore(
                sp.GetRequiredService<TMasterDbContext>(),
                sp.GetRequiredService<IMemoryCache>()
            )
        );
        services.AddScoped<IUserAccessStampStore>(sp =>
            new MasterUserAccessStampStore(
                sp.GetRequiredService<TMasterDbContext>(),
                sp.GetRequiredService<IMemoryCache>()
            )
        );

        if (validatorFactory is null)
            services.AddScoped<IAccessTokenSessionValidator, ClaimAccessTokenSessionValidator>();
        else
            services.AddScoped(validatorFactory);

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PermissionDeniedAuthorizationResultHandler>();

        return services;
    }

    /// <summary>Master tablolarını oluşturur (migrate-master).</summary>
    public static Task EnsureAccessTokenRevocationSchemaAsync(
        this DatabaseFacade database,
        CancellationToken cancellationToken = default
    ) =>
        database.ExecuteSqlRawAsync(
            AccessTokenRevocationSchemaSql.EnsureTables,
            cancellationToken
        );
}
