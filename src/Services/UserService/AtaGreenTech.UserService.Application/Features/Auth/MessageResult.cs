namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Basit başarı/hata mesajı (verify-email gibi yazma komutları için).</summary>
public sealed record MessageResult(bool Succeeded, string Message)
{
    public static MessageResult Ok(string message) => new(true, message);

    public static MessageResult Fail(string message) => new(false, message);
}
