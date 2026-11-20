using System.Security.Claims;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>JWT imza/exp sonrası oturum + stamp + denylist doğrulaması (#70).</summary>
public interface IAccessTokenSessionValidator
{
    Task<AccessTokenSessionValidationResult> ValidateAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default
    );
}

public sealed record AccessTokenSessionValidationResult(
    bool Succeeded,
    string? ErrorCode = null,
    string? Message = null
)
{
    public static AccessTokenSessionValidationResult Ok() => new(true);

    public static AccessTokenSessionValidationResult Fail(string errorCode, string message) =>
        new(false, errorCode, message);
}
