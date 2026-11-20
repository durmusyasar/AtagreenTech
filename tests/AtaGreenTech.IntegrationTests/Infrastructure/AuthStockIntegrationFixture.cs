using AtaGreenTech.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace AtaGreenTech.IntegrationTests.Infrastructure;

/// <summary>
/// Paylaşılan Postgres (Testcontainers) + User/Stock WebApplicationFactory.
/// Master migrate → tenant onboard (User migrations) → Stock tenant migrations.
/// </summary>
public sealed class AuthStockIntegrationFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;

    public UserServiceTestFactory UserFactory { get; private set; } = null!;
    public StockServiceTestFactory StockFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("atagreen_e2e_master")
            .WithUsername("atagreen")
            .WithPassword("atagreen_e2e")
            .Build();

        await _postgres.StartAsync();

        var masterCs = _postgres.GetConnectionString();
        var tenantCs = new NpgsqlConnectionStringBuilder(masterCs)
        {
            Database = "atagreen_e2e_tenant",
        }.ConnectionString;

        UserFactory = new UserServiceTestFactory(masterCs);
        StockFactory = new StockServiceTestFactory(masterCs);

        await using (var scope = UserFactory.Services.CreateAsyncScope())
        {
            var master = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            await master.Database.MigrateAsync();
            await master.Database.EnsureAccessTokenRevocationSchemaAsync();

            var onboarding = scope.ServiceProvider.GetRequiredService<ITenantOnboardingService>();
            await onboarding.RegisterTenantAsync(
                new RegisterTenantRequest(
                    SharedJwtContract.TenantSubdomain,
                    "E2E Tenant",
                    tenantCs
                )
            );
        }

        // Stock katalog tabloları security-test uçları için gerekmez; yine de Stock migration'larını uygula
        // ki iki servisin aynı tenant DB sözleşmesi CI'da doğrulansın.
        await using (var scope = StockFactory.Services.CreateAsyncScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<ITenantMigrationRunner>();
            var summary = await runner.MigrateAllAsync();
            if (summary.HasFailures)
                throw new InvalidOperationException("Stock tenant migrations failed during E2E fixture setup.");
        }
    }

    public async Task DisposeAsync()
    {
        if (StockFactory is not null)
            await StockFactory.DisposeAsync();
        if (UserFactory is not null)
            await UserFactory.DisposeAsync();
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }

    public HttpClient CreateUserClient()
    {
        var client = UserFactory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            }
        );
        client.DefaultRequestHeaders.Host = SharedJwtContract.TenantHost;
        return client;
    }

    public HttpClient CreateStockClient()
    {
        var client = StockFactory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            }
        );
        client.DefaultRequestHeaders.Host = SharedJwtContract.TenantHost;
        return client;
    }
}

[CollectionDefinition(Name)]
public sealed class AuthStockIntegrationCollection : ICollectionFixture<AuthStockIntegrationFixture>
{
    public const string Name = "AuthStockIntegration";
}
