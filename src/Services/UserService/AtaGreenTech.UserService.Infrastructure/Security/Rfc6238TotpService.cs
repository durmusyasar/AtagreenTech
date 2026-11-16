using System.Globalization;
using System.Security.Cryptography;
using AtaGreenTech.UserService.Application.Interfaces;

namespace AtaGreenTech.UserService.Infrastructure.Security;

/// <summary>HMAC-SHA1 tabanlı 6 haneli TOTP (30 sn adım, ±1 pencere).</summary>
internal sealed class Rfc6238TotpService : ITotpService
{
    private const int StepSeconds = 30;
    private const int Digits = 6;
    private const int Window = 1;

    public string GenerateSecret() =>
        Base32Encoding.ToString(RandomNumberGenerator.GetBytes(20));

    public string BuildAuthenticatorUri(string email, string secretBase32, string issuer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretBase32);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);

        var label = $"{issuer.Trim()}:{email.Trim()}";
        return string.Concat(
            "otpauth://totp/",
            Uri.EscapeDataString(label),
            "?secret=",
            Uri.EscapeDataString(secretBase32.Trim()),
            "&issuer=",
            Uri.EscapeDataString(issuer.Trim()),
            "&digits=",
            Digits.ToString(CultureInfo.InvariantCulture)
        );
    }

    public bool VerifyCode(string secretBase32, string code)
    {
        if (string.IsNullOrWhiteSpace(secretBase32) || string.IsNullOrWhiteSpace(code))
            return false;

        var normalized = new string(code.Where(char.IsDigit).ToArray());
        if (normalized.Length != Digits)
            return false;

        byte[] key;
        try
        {
            key = Base32Encoding.ToBytes(secretBase32);
        }
        catch (FormatException)
        {
            return false;
        }

        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;
        for (var i = -Window; i <= Window; i++)
        {
            if (FixedTimeEquals(ComputeTotp(key, timestep + i), normalized))
                return true;
        }

        return false;
    }

    private static string ComputeTotp(byte[] key, long timestep)
    {
        Span<byte> counter = stackalloc byte[8];
        for (var i = 7; i >= 0; i--)
        {
            counter[i] = (byte)(timestep & 0xff);
            timestep >>= 8;
        }

        Span<byte> hash = stackalloc byte[HMACSHA1.HashSizeInBytes];
        HMACSHA1.HashData(key, counter, hash);

        var offset = hash[^1] & 0x0f;
        var binary =
            ((hash[offset] & 0x7f) << 24)
            | ((hash[offset + 1] & 0xff) << 16)
            | ((hash[offset + 2] & 0xff) << 8)
            | (hash[offset + 3] & 0xff);

        var otp = binary % (int)Math.Pow(10, Digits);
        return otp.ToString($"D{Digits}", CultureInfo.InvariantCulture);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        var result = 0;
        for (var i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0;
    }
}
