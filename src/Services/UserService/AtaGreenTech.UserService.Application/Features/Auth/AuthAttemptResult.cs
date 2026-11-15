namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Auth komutlarının başarı/hata sonucu. Lockout sayacı ve refresh reuse detection gibi DB'ye
/// yazılması gereken güvenlik işlemlerinde exception fırlatılmaz; aksi halde OmniSync
/// <c>TransactionBehavior</c> rollback değişiklikleri geri alır.
/// </summary>
public sealed record AuthAttemptResult(
    AuthResponse? Response,
    string? ErrorMessage,
    string? ErrorCode = null,
    int StatusCode = 401
)
{
    public bool IsSuccess => Response is not null;

    public static AuthAttemptResult Ok(AuthResponse response) => new(response, null);

    public static AuthAttemptResult Fail(string errorMessage) => new(null, errorMessage);

    public static AuthAttemptResult Fail(string errorCode, string errorMessage, int statusCode = 401) =>
        new(null, errorMessage, errorCode, statusCode);
}
