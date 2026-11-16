using System.Text;

namespace AtaGreenTech.UserService.Infrastructure.Security;

/// <summary>RFC 4648 Base32 (Authenticator uygulamalarıyla uyumlu, padding'siz).</summary>
internal static class Base32Encoding
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string ToString(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
            return string.Empty;

        var output = new StringBuilder((data.Length + 4) / 5 * 8);
        int buffer = data[0];
        var next = 1;
        var bitsLeft = 8;

        while (bitsLeft > 0 || next < data.Length)
        {
            if (bitsLeft < 5)
            {
                if (next < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[next++] & 0xff;
                    bitsLeft += 8;
                }
                else
                {
                    var pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }

            var index = (buffer >> (bitsLeft - 5)) & 0x1f;
            bitsLeft -= 5;
            output.Append(Alphabet[index]);
        }

        return output.ToString();
    }

    public static byte[] ToBytes(string base32)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base32);
        var cleaned = new string(base32.Trim().Where(c => c != '=').Select(char.ToUpperInvariant).ToArray());
        if (cleaned.Length == 0)
            throw new FormatException("Base32 input is empty.");

        var output = new byte[cleaned.Length * 5 / 8];
        var buffer = 0;
        var bitsLeft = 0;
        var index = 0;

        foreach (var c in cleaned)
        {
            var val = Alphabet.IndexOf(c);
            if (val < 0)
                throw new FormatException($"Invalid Base32 character: {c}");

            buffer = (buffer << 5) | val;
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                output[index++] = (byte)((buffer >> (bitsLeft - 8)) & 0xff);
                bitsLeft -= 8;
            }
        }

        return output;
    }
}
