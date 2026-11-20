using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.SharedKernel.Localization;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Kimlik challenge jetonları (e-posta doğrulama, şifre sıfırlama, davet).
    /// Düz metin yalnızca e-posta linkinde; DB’de SHA-256 hex özeti tutulur.
    /// Geçersiz girdilerde <see cref="ErrorCodes"/> taşınır (#71).
    /// </summary>
    internal static class EmailVerificationTokenFactory
    {
        private const int PlaintextEntropyBytes = 32;

        /// <summary>Düz metin jetonun SHA-256 özetini büyük harf hex olarak üretir.</summary>
        public static string Hash(string plaintextToken)
        {
            if (string.IsNullOrWhiteSpace(plaintextToken))
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(plaintextToken));

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plaintextToken)));
        }

        /// <summary>32 bayt entropili Base64 plaintext + eşleşen hash üretir.</summary>
        public static (string Plaintext, string Hash) Create()
        {
            var plaintext = Convert.ToBase64String(RandomNumberGenerator.GetBytes(PlaintextEntropyBytes));
            return (plaintext, Hash(plaintext));
        }
    }
}