namespace AtaGreenTech.UserService.Domain;

/// <summary>
/// Domain invariant hata kodları. Değerler SharedKernel <c>ErrorCodes</c> / <c>SharedResource</c> ile birebir (#71).
/// Domain, SharedKernel’e (AspNetCore/Application) referans vermez; API sınırı aynı kodları lokalize eder.
/// </summary>
public static class DomainErrorCodes
{
    public const string ValidationRequired = "VALIDATION_REQUIRED";
    public const string ValidationMaxLength = "VALIDATION_MAX_LENGTH";
    public const string ValidationInvalid = "VALIDATION_INVALID";
    public const string ValidationGreaterThanZero = "VALIDATION_GREATER_THAN_ZERO";

    public const string AuthMfaAlreadyEnabled = "AUTH_MFA_ALREADY_ENABLED";
    public const string AuthMfaSetupRequired = "AUTH_MFA_SETUP_REQUIRED";
    public const string AuthInviteTokenInvalid = "AUTH_INVITE_TOKEN_INVALID";
}
