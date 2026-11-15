using System.Security.Cryptography;
using System.Text;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// E-posta doğrulama token üretimi. Düz metin yalnızca e-posta linkinde; DB'de SHA-256 özeti tutulur.
/// </summary>
internal static class EmailVerificationTokenFactory
{
    public static string Hash(string plaintextToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintextToken);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextToken)));
    }

    public static (string Plaintext, string Hash) Create()
    {
        var plaintext = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (plaintext, Hash(plaintext));
    }
}
