using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Refresh token üretimi ve SHA-256 özetleme. Düz metin yalnızca istemciye verilir;
    /// DB’de <see cref="RefreshToken.TokenHash"/> (hex) saklanır. <c>familyId</c> access token <c>sid</c> ile aynıdır (#70).
    /// </summary>
    internal static class RefreshTokenFactory
    {
        private const int PlaintextEntropyBytes = 32;

        /// <summary>
        /// Düz metin refresh token’ın SHA-256 özetini büyük harf hex olarak üretir (arama / reuse detection).
        /// </summary>
        public static string Hash(string plaintextToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(plaintextToken);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextToken)));
        }

        /// <summary>
        /// Yeni düz metin token + <see cref="RefreshToken"/> entity üretir (32 bayt entropi, Base64 plaintext).
        /// </summary>
        /// <param name="userId">Token sahibi.</param>
        /// <param name="familyId">Oturum / refresh ailesi (<c>sid</c>); rotation’da sabit kalır.</param>
        /// <param name="refreshTokenDays">Ömür (gün); 1 veya daha büyük olmalıdır.</param>
        /// <param name="clientIp">Oluşturan istemci IP (opsiyonel).</param>
        /// <param name="deviceInfo">User-Agent / cihaz özeti (opsiyonel).</param>
        public static (string Plaintext, RefreshToken Entity) Create(
            Guid userId,
            Guid familyId,
            int refreshTokenDays,
            string? clientIp,
            string? deviceInfo = null
        )
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User id is required.", nameof(userId));
            if (familyId == Guid.Empty)
                throw new ArgumentException("Family id is required.", nameof(familyId));
            if (refreshTokenDays <= 0)
                throw new ArgumentOutOfRangeException(nameof(refreshTokenDays),
                    "Refresh token lifetime must be greater than zero.");

            var plaintext = Convert.ToBase64String(RandomNumberGenerator.GetBytes(PlaintextEntropyBytes));
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
}