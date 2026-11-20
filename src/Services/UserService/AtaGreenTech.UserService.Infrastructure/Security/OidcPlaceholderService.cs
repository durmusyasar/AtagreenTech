using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;

namespace AtaGreenTech.UserService.Infrastructure.Security
{
    /// <summary>
    /// V1 OIDC SSO iskeleti. Tüm çağrılar <see cref="NotImplementedException"/> +
    /// <see cref="ErrorCodes.SsoNotImplemented"/> → API 501 + lokalize (#71).
    /// Feature-flag / sağlayıcı ayarları V2 gerçek implementasyonda kullanılır.
    /// </summary>
    internal sealed class OidcPlaceholderService : IOidcAuthService
    {
        /// <inheritdoc />
        public Task<OidcAuthorizationUrlResponse> GetAuthorizationUrlAsync(
            string provider,
            string redirectUri,
            CancellationToken cancellationToken = default
        ) =>
            throw SsoNotImplemented();

        /// <inheritdoc />
        public Task<AuthResponse> HandleCallbackAsync(
            string provider,
            OidcCallbackRequest request,
            CancellationToken cancellationToken = default
        )
        {
            ArgumentNullException.ThrowIfNull(request);
            throw SsoNotImplemented();
        }

        private static NotImplementedException SsoNotImplemented() =>
            new(ErrorCodes.SsoNotImplemented);
    }
}