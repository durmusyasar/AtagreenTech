namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>
/// OmniSync <c>JwtSettings</c> ile uyumlu access token üretir (HS256 SigningKey veya RS256 private key).
/// </summary>
public interface IAccessTokenIssuer
{
    AccessTokenResult Create(
        Guid userId,
        string email,
        Guid tenantId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions
    );
}

/// <param name="AccessToken">İmzalı JWT.</param>
/// <param name="ExpiresAt">UTC son kullanma zamanı.</param>
public sealed record AccessTokenResult(string AccessToken, DateTime ExpiresAt);
