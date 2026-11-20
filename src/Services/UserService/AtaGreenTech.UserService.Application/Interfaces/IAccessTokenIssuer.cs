namespace AtaGreenTech.UserService.Application.Interfaces
{
    /// <summary>
    /// OmniSync <c>JwtSettings</c> ile uyumlu access token üretir (HS256 SigningKey veya RS256 private key).
    /// </summary>
    public interface IAccessTokenIssuer
    {
        /// <summary>
        /// <see cref="AtaGreenTech.SharedKernel.Security.JwtContract"/> claim’leriyle imzalı access token üretir (#70: <c>sid</c>/<c>sst</c>).
        /// </summary>
        /// <param name="userId">JWT <c>sub</c>.</param>
        /// <param name="email">JWT <c>email</c>.</param>
        /// <param name="tenantId">JWT <c>tenant_id</c> (yalnızca doğrulanmış tenant kapsamı).</param>
        /// <param name="sessionId">JWT <c>sid</c> — refresh ailesi / oturum kimliği.</param>
        /// <param name="securityStamp">JWT <c>sst</c> — <c>User.SecurityStamp</c> anlık görüntüsü.</param>
        /// <param name="roles">JWT <c>role</c> (çoklu; boş/whitespace atılır, distinct).</param>
        /// <param name="permissions">JWT <c>permission</c> (çoklu; boş/whitespace atılır, distinct).</param>
        AccessTokenResult Create(
            Guid userId,
            string email,
            Guid tenantId,
            Guid sessionId,
            string securityStamp,
            IReadOnlyCollection<string> roles,
            IReadOnlyCollection<string> permissions
        );
    }

    /// <param name="AccessToken">İmzalı JWT.</param>
    /// <param name="ExpiresAt">UTC son kullanma zamanı.</param>
    public sealed record AccessTokenResult(string AccessToken, DateTime ExpiresAt);
}