namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Basit başarı/hata sonucu. Hatalarda <see cref="ErrorCode"/> taşıyarak API sınırında yerelleştirilir (#71).</summary>
public sealed record MessageResult(
    bool Succeeded,
    string Message,
    string? ErrorCode = null,
    IReadOnlyList<object>? ErrorArgs = null
)
{
    public static MessageResult Ok(string message) => new(true, message);

    public static MessageResult Fail(string message) => new(false, message);

    public static MessageResult Fail(string errorCode, string? fallbackMessage = null, params object[] args) =>
        new(false, fallbackMessage ?? errorCode, errorCode, args);
}
