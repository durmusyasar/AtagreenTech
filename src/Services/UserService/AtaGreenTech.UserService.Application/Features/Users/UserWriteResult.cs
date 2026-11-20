using AtaGreenTech.UserService.Application.DTOs;

namespace AtaGreenTech.UserService.Application.Features.Users;

/// <summary>Kullanıcı yazma komutları için başarı/hata + isteğe bağlı DTO. Hatalarda ErrorCode API sınırında yerelleştirilir (#71).</summary>
public sealed record UserWriteResult(
    bool Succeeded,
    string Message,
    UserDto? User = null,
    string? ErrorCode = null,
    IReadOnlyList<object>? ErrorArgs = null
)
{
    public static UserWriteResult Ok(UserDto user, string message = "") => new(true, message, user);

    public static UserWriteResult Fail(string message) => new(false, message);

    public static UserWriteResult Fail(string errorCode, string? fallbackMessage = null, params object[] args) =>
        new(false, fallbackMessage ?? errorCode, null, errorCode, args);
}
