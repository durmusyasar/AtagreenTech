using AtaGreenTech.SharedKernel.Localization;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Kimlik doğrulama uç noktaları için tutarlı hata kodları.
    /// Değerler <see cref="ErrorCodes"/> ile aynıdır (#71 AUTH_* korunur).
    /// </summary>
    public static class AuthErrorCodes
    {
        public const string InvalidCredentials = ErrorCodes.AuthInvalidCredentials;
        public const string AccountLocked = ErrorCodes.AuthAccountLocked;
        public const string AccountInactive = ErrorCodes.AuthAccountInactive;
        public const string EmailNotVerified = ErrorCodes.AuthEmailNotVerified;
        public const string RateLimitExceeded = ErrorCodes.AuthRateLimitExceeded;
        public const string MfaRequired = ErrorCodes.AuthMfaRequired;
        public const string MfaInvalid = ErrorCodes.AuthMfaInvalid;
        public const string TenantUnresolved = ErrorCodes.AuthTenantUnresolved;
        public const string SsoNotImplemented = ErrorCodes.SsoNotImplemented;
    }
}