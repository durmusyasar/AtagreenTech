using System.Threading.RateLimiting;
using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Api.Contracts;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Options;

namespace AtaGreenTech.UserService.Api.Hosting
{
    /// <summary>Login / şifre sıfırlama / davet kabul gibi brute-force'a açık uçlar için sabit pencere limiti.</summary>
    public static class AuthRateLimiting
    {
        public const string PolicyName = "AuthRateLimit";

        public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            var authSection = configuration.GetSection(AuthOptions.SectionName);
            var permitLimit = authSection.GetValue(nameof(AuthOptions.AuthRateLimitPermitLimit), 5);
            var windowMinutes = authSection.GetValue(nameof(AuthOptions.AuthRateLimitWindowMinutes), 1);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(
                    PolicyName,
                    httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = permitLimit,
                                Window = TimeSpan.FromMinutes(windowMinutes),
                                QueueLimit = 0,
                            }
                        )
                );

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var localizer = context.HttpContext.RequestServices.GetRequiredService<IErrorLocalizer>();
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new ErrorResponse(
                            AuthErrorCodes.RateLimitExceeded,
                            localizer.Localize(AuthErrorCodes.RateLimitExceeded)
                        ),
                        cancellationToken
                    );
                };
            });

            return services;
        }
    }
}
