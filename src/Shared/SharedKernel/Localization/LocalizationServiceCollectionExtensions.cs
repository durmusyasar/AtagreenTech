using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace AtaGreenTech.SharedKernel.Localization;

public static class LocalizationServiceCollectionExtensions
{
    public static readonly string[] SupportedCultureCodes = ["tr", "en"];

    public const string DefaultCultureCode = "tr";

    /// <summary>
    /// <see cref="IStringLocalizer{SharedResource}"/>, <see cref="IErrorLocalizer"/> ve
    /// <see cref="RequestLocalizationOptions"/> (Accept-Language → tr/en, default tr).
    /// </summary>
    public static IServiceCollection AddAtaGreenLocalization(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddSingleton<IErrorLocalizer, StringLocalizerErrorLocalizer>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultures = SupportedCultureCodes
                .Select(c => new CultureInfo(c))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture(DefaultCultureCode);
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            options.ApplyCurrentCultureToResponseHeaders = true;

            // Desteklenmeyen dil → default (tr); hata fırlatılmaz.
            options.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider(),
            ];
        });

        return services;
    }
}
