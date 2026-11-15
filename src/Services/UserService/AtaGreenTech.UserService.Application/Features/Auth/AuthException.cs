namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Kodlu kimlik doğrulama hatası. Login/refresh gibi transaction içinde kalıcı yazım gereken
/// akışlarda <see cref="AuthAttemptResult"/> tercih edilir; aksi halde rollback lockout'u siler.
/// </summary>
public sealed class AuthException : Exception
{
    public AuthException(string code, string message, int statusCode = 401)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }

    public int StatusCode { get; }
}
