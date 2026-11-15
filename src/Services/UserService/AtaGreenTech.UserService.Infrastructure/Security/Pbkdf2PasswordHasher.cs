using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.UserService.Application.Interfaces;

namespace AtaGreenTech.UserService.Infrastructure.Security;

/// <summary>
/// PBKDF2-HMAC-SHA256 (100.000 iterasyon) ile parola özetler. Format: <c>{saltBase64}.{hashBase64}</c>.
/// Doğrulama <see cref="CryptographicOperations.FixedTimeEquals"/> ile zamanlama sızıntısına karşı korunur.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int IterationCount = 100_000;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Derive(password, salt);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var parts = storedHash.Split('.', 2);
        if (parts.Length != 2)
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            expected = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length != SaltSizeBytes || expected.Length != HashSizeBytes)
            return false;

        var actual = Derive(password, salt);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Derive(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            IterationCount,
            HashAlgorithmName.SHA256,
            HashSizeBytes
        );
}
