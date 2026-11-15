using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Refresh token üretimi ve SHA-256 özetleme. Düz metin yalnızca istemciye verilir.
/// </summary>
internal static class RefreshTokenFactory
{
    public static string Hash(string plaintextToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintextToken);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextToken)));
    }

    public static (string Plaintext, RefreshToken Entity) Create(
        Guid userId,
        Guid familyId,
        int refreshTokenDays,
        string? clientIp,
        string? deviceInfo = null
    )
    {
        var plaintext = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var entity = new RefreshToken(
            userId,
            Hash(plaintext),
            familyId,
            DateTime.UtcNow.AddDays(refreshTokenDays),
            clientIp,
            deviceInfo
        );
        return (plaintext, entity);
    }
}

