using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.UserService.Application.Interfaces;

namespace AtaGreenTech.UserService.Infrastructure.Security
{
    /// <summary>
    /// PBKDF2-HMAC-SHA256 (<see cref="IterationCount"/> iterasyon) ile parola özetler.
    /// Yeni özetler: <c>v1.{saltBase64}.{hashBase64}</c>. Eski <c>{salt}.{hash}</c> doğrulamada kabul edilir.
    /// Doğrulama <see cref="CryptographicOperations.FixedTimeEquals"/> ile zamanlama sızıntısına karşı korunur.
    /// </summary>
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const string FormatVersionPrefix = "v1";
        private const int SaltSizeBytes = 16;
        private const int HashSizeBytes = 32;
        private const int IterationCount = 100_000;

        /// <inheritdoc />
        public string Hash(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            var hash = Derive(password, salt);
            return $"{FormatVersionPrefix}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <inheritdoc />
        public bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            if (!TryParseStoredHash(storedHash, out var salt, out var expected))
                return false;

            if (salt.Length != SaltSizeBytes || expected.Length != HashSizeBytes)
                return false;

            var actual = Derive(password, salt);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        /// <summary>
        /// <c>v1.salt.hash</c> veya legacy <c>salt.hash</c> biçimini çözer.
        /// </summary>
        private static bool TryParseStoredHash(string storedHash, out byte[] salt, out byte[] expected)
        {
            salt = [];
            expected = [];

            var parts = storedHash.Split('.');
            string saltPart;
            string hashPart;

            if (parts.Length == 3
                && parts[0].Equals(FormatVersionPrefix, StringComparison.Ordinal))
            {
                saltPart = parts[1];
                hashPart = parts[2];
            }
            else if (parts.Length == 2)
            {
                // Legacy: {saltBase64}.{hashBase64}
                saltPart = parts[0];
                hashPart = parts[1];
            }
            else
            {
                return false;
            }

            try
            {
                salt = Convert.FromBase64String(saltPart);
                expected = Convert.FromBase64String(hashPart);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>PBKDF2-HMAC-SHA256 türetmesi.</summary>
        private static byte[] Derive(string password, byte[] salt) =>
            Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                IterationCount,
                HashAlgorithmName.SHA256,
                HashSizeBytes
            );
    }
}