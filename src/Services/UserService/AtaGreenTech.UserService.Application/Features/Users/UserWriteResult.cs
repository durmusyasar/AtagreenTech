using AtaGreenTech.UserService.Application.DTOs;

namespace AtaGreenTech.UserService.Application.Features.Users;

/// <summary>
/// Kullanıcı yazma komutları için başarı/hata + isteğe bağlı DTO.
/// Hatalarda <see cref="ErrorCode"/> API sınırında yerelleştirilir (#71).
/// </summary>
public sealed record UserWriteResult(
    bool Succeeded,
    string Message,
    UserDto? User = null,
    string? ErrorCode = null,
    IReadOnlyList<object>? ErrorArgs = null
)
{
    public static UserWriteResult Ok(UserDto user, string message = "") => new(true, message, user);

    /// <summary>
    /// Hata kodu taşır. Tek argüman her zaman <see cref="ErrorCode"/> olarak yazılır
    /// (eski <c>Fail(string message)</c> aşırı yüklemesi kaldırıldı — aksi halde kod kayboluyordu).
    /// </summary>
    public static UserWriteResult Fail(string errorCode, string? fallbackMessage = null, params object[] args) =>
        new(false, fallbackMessage ?? errorCode, null, errorCode, args);
}
