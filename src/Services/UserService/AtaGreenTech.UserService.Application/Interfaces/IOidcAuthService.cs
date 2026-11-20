using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Application.Interfaces
{
    /// <summary>
    /// OIDC SSO sözleşmesi. V1 placeholder; V2’de gerçek sağlayıcı akışları eklenecek.
    /// V1’de desteklenmeyen yollar <see cref="System.NotImplementedException"/> +
    /// <c>SSO_NOT_IMPLEMENTED</c> taşır; API 501 + <c>IErrorLocalizer</c> (#71).
    /// </summary>
    public interface IOidcAuthService
    {
        /// <summary>Sağlayıcı authorization URL + state üretir (V1: henüz aktif değil).</summary>
        /// <param name="provider">Yapılandırılmış OIDC sağlayıcı adı (örn. Google).</param>
        /// <param name="redirectUri">İstemci dönüş URI’si.</param>
        /// <param name="cancellationToken">İptal jetonu.</param>
        Task<OidcAuthorizationUrlResponse> GetAuthorizationUrlAsync(
            string provider,
            string redirectUri,
            CancellationToken cancellationToken = default
        );

        /// <summary>Authorization code callback’ini işler (V1: henüz aktif değil).</summary>
        /// <param name="provider">Yapılandırılmış OIDC sağlayıcı adı.</param>
        /// <param name="request">Code + state gövdesi.</param>
        /// <param name="cancellationToken">İptal jetonu.</param>
        Task<AuthResponse> HandleCallbackAsync(
            string provider,
            OidcCallbackRequest request,
            CancellationToken cancellationToken = default
        );
    }

    /// <summary>Sağlayıcıya yönlendirme için authorization URL + state.</summary>
    public sealed record OidcAuthorizationUrlResponse(string Provider, string AuthorizationUrl, string State);

    /// <summary>OIDC callback gövdesi (authorization code + state).</summary>
    public sealed record OidcCallbackRequest(string Code, string State);
}