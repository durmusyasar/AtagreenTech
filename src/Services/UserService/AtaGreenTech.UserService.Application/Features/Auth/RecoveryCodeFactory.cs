using System.Security.Cryptography;
using System.Text;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Tek kullanımlık MFA kurtarma kodları; DB'de yalnızca SHA-256 özeti tutulur.</summary>
internal static class RecoveryCodeFactory
{
    public const int DefaultCount = 8;
    public const int CodeLength = 8;

    public static (IReadOnlyList<string> Plaintext, IReadOnlyList<string> Hashes) Create(int count = DefaultCount)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var plaintext = new string[count];
        var hashes = new string[count];
        for (var i = 0; i < count; i++)
        {
            plaintext[i] = CreateCode();
            hashes[i] = Hash(plaintext[i]);
        }

        return (plaintext, hashes);
    }

    public static string Hash(string plaintextCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintextCode);
        var normalized = Normalize(plaintextCode);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    public static string Normalize(string code) =>
        new string(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    private static string CreateCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<byte> bytes = stackalloc byte[CodeLength];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[CodeLength];
        for (var i = 0; i < CodeLength; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        return new string(chars);
    }
}
