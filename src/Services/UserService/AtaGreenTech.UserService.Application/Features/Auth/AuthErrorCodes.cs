namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Kimlik doğrulama uç noktaları için tutarlı hata kodları.</summary>
public static class AuthErrorCodes
{
    public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AccountInactive = "AUTH_ACCOUNT_INACTIVE";
    public const string EmailNotVerified = "AUTH_EMAIL_NOT_VERIFIED";
    public const string RateLimitExceeded = "AUTH_RATE_LIMIT_EXCEEDED";
    public const string MfaRequired = "AUTH_MFA_REQUIRED";
    public const string MfaInvalid = "AUTH_MFA_INVALID";
}
