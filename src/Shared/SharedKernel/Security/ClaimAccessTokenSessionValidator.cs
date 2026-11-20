using System.Security.Claims;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Ortak claim + denylist + Master stamp doğrulaması. UserService ek olarak tenant User durumunu kontrol eder.
/// </summary>
public class ClaimAccessTokenSessionValidator(
    IAccessTokenRevocationStore revocations,
    IUserAccessStampStore stamps
) : IAccessTokenSessionValidator
{
    public const string JtiClaim = "jti";

    public virtual async Task<AccessTokenSessionValidationResult> ValidateAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userIdRaw = principal.FindFirstValue(JwtContract.Claims.Subject)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var tenantRaw = principal.FindFirstValue(JwtContract.Claims.TenantId);
        var sessionRaw = principal.FindFirstValue(JwtContract.Claims.SessionId);
        var stampClaim = principal.FindFirstValue(JwtContract.Claims.SecurityStamp);
        var jti = principal.FindFirstValue(JtiClaim);

        if (!Guid.TryParse(userIdRaw, out var userId)
            || !Guid.TryParse(tenantRaw, out var tenantId)
            || !Guid.TryParse(sessionRaw, out var sessionId)
            || string.IsNullOrWhiteSpace(stampClaim))
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "Access token is missing required session claims."
            );
        }

        if (!string.IsNullOrWhiteSpace(jti)
            && await revocations.IsTokenRevokedAsync(jti, cancellationToken))
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "Access token has been revoked."
            );
        }

        if (await revocations.IsSessionRevokedAsync(sessionId, cancellationToken))
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "Session has been revoked."
            );
        }

        var published = await stamps.GetAsync(tenantId, userId, cancellationToken);
        if (published is null
            || !string.Equals(published, stampClaim.Trim(), StringComparison.Ordinal))
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "Security stamp is no longer valid."
            );
        }

        return AccessTokenSessionValidationResult.Ok();
    }
}
