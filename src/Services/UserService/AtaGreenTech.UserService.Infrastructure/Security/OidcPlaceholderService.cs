using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Infrastructure.Security;

/// <summary>
/// V1 OIDC SSO iskeleti. Feature-flag kapalıysa veya V1 kapsamında implemente edilmemişse
/// <see cref="NotImplementedException"/> fırlatır; API bunu 501'e map eder.
/// </summary>
internal sealed class OidcPlaceholderService(
    IOptions<FeatureFlagsOptions> featureFlags,
    IOptions<OidcOptions> oidcOptions
) : IOidcAuthService
{
    public Task<OidcAuthorizationUrlResponse> GetAuthorizationUrlAsync(
        string provider,
        string redirectUri,
        CancellationToken cancellationToken = default
    )
    {
        EnsureSsoEnabled();
        EnsureKnownProvider(provider);
        throw new NotImplementedException(
            $"OIDC sağlayıcısı ('{NormalizeProvider(provider)}') henüz V1 sürümünde aktif değildir."
        );
    }

    public Task<AuthResponse> HandleCallbackAsync(
        string provider,
        OidcCallbackRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureSsoEnabled();
        EnsureKnownProvider(provider);
        throw new NotImplementedException(
            $"OIDC callback akışı ('{NormalizeProvider(provider)}') henüz V1 sürümünde aktif değildir."
        );
    }

    private void EnsureSsoEnabled()
    {
        if (!featureFlags.Value.EnableOidcSso)
        {
            throw new NotImplementedException(
                "SSO / OpenID Connect özelliği sistem yapılandırmasında devre dışı (Feature-Flag kapalı)."
            );
        }
    }

    private void EnsureKnownProvider(string provider)
    {
        if (oidcOptions.Value.TryGetProvider(provider, out _))
            return;

        throw new NotImplementedException(
            $"Bilinmeyen veya yapılandırılmamış OIDC sağlayıcısı: '{NormalizeProvider(provider)}'."
        );
    }

    private static string NormalizeProvider(string provider) =>
        string.IsNullOrWhiteSpace(provider) ? string.Empty : provider.Trim();
}
