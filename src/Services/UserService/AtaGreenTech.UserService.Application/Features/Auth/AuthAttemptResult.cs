namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Auth komutlarının başarı/hata sonucu. Lockout sayacı, recovery code tüketimi ve refresh reuse
/// detection gibi DB yazımlarında exception fırlatılmaz; aksi halde OmniSync transaction rollback eder.
/// </summary>
public sealed record AuthAttemptResult(
    AuthResponse? Response,
    string? ErrorMessage,
    string? ErrorCode = null,
    int StatusCode = 401,
    bool MfaRequired = false,
    IReadOnlyList<object>? ErrorArgs = null
)
{
    public bool IsSuccess => Response is not null;

    public static AuthAttemptResult Ok(AuthResponse response) => new(response, null);

    public static AuthAttemptResult Fail(string errorMessage) => new(null, errorMessage);

    public static AuthAttemptResult Fail(string errorCode, string errorMessage, int statusCode = 401) =>
        new(null, errorMessage, errorCode, statusCode);

    public static AuthAttemptResult Fail(
        string errorCode,
        string errorMessage,
        int statusCode,
        params object[] args
    ) =>
        new(null, errorMessage, errorCode, statusCode, MfaRequired: false, ErrorArgs: args);

    public static AuthAttemptResult RequireMfa() =>
        new(
            null,
            null,
            AuthErrorCodes.MfaRequired,
            StatusCode: 401,
            MfaRequired: true
        );
}
