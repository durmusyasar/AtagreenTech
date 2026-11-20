using AtaGreenTech.SharedKernel.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AtaGreenTech.UserService.Infrastructure.Security;

internal static class JwtBearerSessionValidationExtensions
{
    public static IServiceCollection AddJwtBearerAccessTokenSessionValidation(
        this IServiceCollection services
    )
    {
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Events ??= new JwtBearerEvents();
            var previous = options.Events.OnTokenValidated;
            options.Events.OnTokenValidated = async context =>
            {
                if (previous is not null)
                    await previous(context);

                if (context.Principal is null || context.Result?.Succeeded == false)
                    return;

                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<IAccessTokenSessionValidator>();
                var result = await validator.ValidateAsync(
                    context.Principal,
                    context.HttpContext.RequestAborted
                );

                if (!result.Succeeded)
                {
                    context.Fail(result.Message ?? AccessTokenErrorCodes.TokenRevoked);
                    context.HttpContext.Items[AccessTokenErrorCodes.TokenRevoked] =
                        result.ErrorCode ?? AccessTokenErrorCodes.TokenRevoked;
                }
            };

            var previousChallenge = options.Events.OnChallenge;
            options.Events.OnChallenge = async context =>
            {
                if (previousChallenge is not null)
                    await previousChallenge(context);

                if (context.Handled)
                    return;

                if (context.HttpContext.Items.TryGetValue(AccessTokenErrorCodes.TokenRevoked, out var code)
                    && code is string errorCode)
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(
                        new
                        {
                            errorCode,
                            detail = context.ErrorDescription
                                ?? "Access token is no longer valid.",
                        }
                    );
                }
            };
        });

        return services;
    }
}
