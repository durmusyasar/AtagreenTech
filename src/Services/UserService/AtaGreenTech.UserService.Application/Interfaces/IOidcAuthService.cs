using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>OIDC SSO sözleşmesi. V1 placeholder; V2'de gerçek sağlayıcı akışları eklenecek.</summary>
public interface IOidcAuthService
{
    Task<OidcAuthorizationUrlResponse> GetAuthorizationUrlAsync(
        string provider,
        string redirectUri,
        CancellationToken cancellationToken = default
    );

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
