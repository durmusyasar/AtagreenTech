namespace AtaGreenTech.SharedKernel.Localization;

/// <summary>
/// Yerelleştirme anahtarları — .resx key ve FE <c>enum.json</c> ile birebir (#71).
/// Auth kodları mevcut <c>AUTH_*</c> sözleşmesini korur.
/// </summary>
public static class ErrorCodes
{
    // Auth & güvenlik (mevcut AUTH_* korunur)
    public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AuthAccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AuthAccountInactive = "AUTH_ACCOUNT_INACTIVE";
    public const string AuthEmailNotVerified = "AUTH_EMAIL_NOT_VERIFIED";
    public const string AuthEmailNotFound = "AUTH_EMAIL_NOT_FOUND";
    public const string AuthRateLimitExceeded = "AUTH_RATE_LIMIT_EXCEEDED";
    public const string AuthMfaRequired = "AUTH_MFA_REQUIRED";
    public const string AuthMfaInvalid = "AUTH_MFA_INVALID";
    public const string SsoNotImplemented = "SSO_NOT_IMPLEMENTED";

    // Oturum / yetki (#70 / #67)
    public const string TokenRevoked = "TOKEN_REVOKED";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string Forbidden = "FORBIDDEN";
    public const string PermissionDenied = "PERMISSION_DENIED";
    public const string ModuleNotSubscribed = "MODULE_NOT_SUBSCRIBED";

    // Genel
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string RecordNotFound = "RECORD_NOT_FOUND";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
}
