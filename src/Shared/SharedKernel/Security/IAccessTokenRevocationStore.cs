namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Access token iptal listesi (jti / sid). Master DB + bellek önbelleği ile UserService ↔ Stock paylaşılır (#70).
/// </summary>
public interface IAccessTokenRevocationStore
{
    Task RevokeSessionAsync(Guid sessionId, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task RevokeTokenAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default);
}
