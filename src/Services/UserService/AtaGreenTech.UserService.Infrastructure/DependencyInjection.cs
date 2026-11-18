using System.Text.RegularExpressions;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Infrastructure.Caching;
using AtaGreenTech.UserService.Infrastructure.Email;
using AtaGreenTech.UserService.Infrastructure.Persistence;
using AtaGreenTech.UserService.Infrastructure.Persistence.Master;
using AtaGreenTech.UserService.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
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
            (services, configuration) =>
            {
                services.AddScoped<IUserDbContext, UserQueryContext>();
                services.AddScoped<INavigationMenuCatalog, NavigationMenuCatalog>();
                services.AddSingleton<INavigationMenuCache, MemoryNavigationMenuCache>();
                services.AddMemoryCache();
                services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
                services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
                services.AddSingleton<ITotpService, Rfc6238TotpService>();
                services.AddScoped<IIdentityAuditLogger, IdentityAuditLogger>();
                services.AddScoped<ITenantIdentitySeeder, TenantIdentitySeeder>();
                services.AddScoped<IOidcAuthService, OidcPlaceholderService>();
                services.AddSingleton<IEmailVerificationSender, LoggingEmailVerificationSender>();
                services.AddSingleton<IPasswordResetSender, LoggingPasswordResetSender>();
                services.AddSingleton<IUserInvitationSender, LoggingUserInvitationSender>();

                services.AddAuthorization(options => options.AddAtaGreenPermissionPolicies());

                services.AddOptions<InternalProvisioningOptions>()
                    .Bind(configuration.GetSection(InternalProvisioningOptions.SectionName))
                    .ValidateOnStart();

                services.AddOptions<FeatureFlagsOptions>()
                    .Bind(configuration.GetSection(FeatureFlagsOptions.SectionName))
                    .ValidateOnStart();

                // OidcProviders altında Google/Microsoft child key'leri doğrudan Providers sözlüğüne bağlanır.
                services.AddOptions<OidcOptions>()
                    .Configure<IConfiguration>((options, config) =>
                    {
                        var section = config.GetSection(OidcOptions.SectionName);
                        foreach (var child in section.GetChildren())
                        {
                            var provider = new OidcProviderConfig();
                            child.Bind(provider);
                            options.Providers[child.Key] = provider;
                        }
                    })
                    .ValidateOnStart();

                services.AddOptions<AuthOptions>()
                    .Bind(configuration.GetSection(AuthOptions.SectionName))
                    .Validate(
                        o => o.AccessTokenMinutes > 0,
                        "Auth:AccessTokenMinutes must be greater than zero."
                    )
                    .Validate(
                        o => o.RefreshTokenDays > 0,
                        "Auth:RefreshTokenDays must be greater than zero."
                    )
                    .Validate(
                        o => o.EmailVerificationHours > 0,
                        "Auth:EmailVerificationHours must be greater than zero."
                    )
                    .Validate(
                        o => o.PasswordResetHours > 0,
                        "Auth:PasswordResetHours must be greater than zero."
                    )
                    .Validate(
                        o => o.InvitationDays > 0,
                        "Auth:InvitationDays must be greater than zero."
                    )
                    .Validate(
                        o => o.MaxFailedAccessAttempts > 0,
                        "Auth:MaxFailedAccessAttempts must be greater than zero."
                    )
                    .Validate(
                        o => o.LockoutMinutes > 0,
                        "Auth:LockoutMinutes must be greater than zero."
                    )
                    .Validate(
                        o => o.AuthRateLimitPermitLimit > 0,
                        "Auth:AuthRateLimitPermitLimit must be greater than zero."
                    )
                    .Validate(
                        o => o.AuthRateLimitWindowMinutes > 0,
                        "Auth:AuthRateLimitWindowMinutes must be greater than zero."
                    )
                    .Validate(
                        o => !string.IsNullOrWhiteSpace(o.MfaIssuer),
                        "Auth:MfaIssuer must not be empty."
                    )
                    .Validate(
                        o => o.MfaRecoveryCodeCount is >= 1 and <= 20,
                        "Auth:MfaRecoveryCodeCount must be between 1 and 20."
                    )
                    .Validate(
                        o => Uri.TryCreate(o.PublicAppBaseUrl, UriKind.Absolute, out var uri)
                            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
                        "Auth:PublicAppBaseUrl must be an absolute http(s) URL."
                    )
                    .Validate(
                        o => string.IsNullOrWhiteSpace(o.Bootstrap.Email)
                            || Regex.IsMatch(o.Bootstrap.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"),
                        "Auth:Bootstrap:Email must be a valid email address when set."
                    )
                    .ValidateOnStart();
            }
        );

        return options;
    }
}
