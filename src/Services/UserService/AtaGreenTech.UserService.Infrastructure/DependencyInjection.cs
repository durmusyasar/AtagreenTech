using System.Text.RegularExpressions;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Infrastructure.Email;
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
            (services, configuration) =>
            {
                services.AddScoped<IUserDbContext, UserQueryContext>();
                services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
                services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
                services.AddSingleton<IEmailVerificationSender, LoggingEmailVerificationSender>();
                services.AddSingleton<IPasswordResetSender, LoggingPasswordResetSender>();

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
