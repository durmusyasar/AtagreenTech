using AtaGreenTech.UserService.Application.DTOs;

namespace AtaGreenTech.UserService.Application.Features.Users;

/// <summary>Kullanıcı yazma komutları için başarı/hata + isteğe bağlı DTO.</summary>
public sealed record UserWriteResult(bool Succeeded, string Message, UserDto? User = null)
{
    public static UserWriteResult Ok(UserDto user, string message = "") => new(true, message, user);

    public static UserWriteResult Fail(string message) => new(false, message);
}
