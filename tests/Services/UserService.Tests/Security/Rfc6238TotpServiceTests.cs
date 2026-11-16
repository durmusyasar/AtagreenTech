using AtaGreenTech.UserService.Infrastructure.Security;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class Rfc6238TotpServiceTests
{
    [Fact]
    public void GenerateSecret_is_base32()
    {
        var sut = new Rfc6238TotpService();
        var secret = sut.GenerateSecret();

        Assert.False(string.IsNullOrWhiteSpace(secret));
        Assert.Matches("^[A-Z2-7]+$", secret);
    }

    [Fact]
    public void BuildAuthenticatorUri_contains_issuer_and_secret()
    {
        var sut = new Rfc6238TotpService();
        var uri = sut.BuildAuthenticatorUri("admin@omnisync.com", "JBSWY3DPEHPK3PXP", "OmniSync");

        Assert.StartsWith("otpauth://totp/", uri);
        Assert.Contains("secret=JBSWY3DPEHPK3PXP", uri);
        Assert.Contains("issuer=OmniSync", uri);
    }

    [Fact]
    public void VerifyCode_accepts_current_window_code()
    {
        var sut = new Rfc6238TotpService();
        var secret = sut.GenerateSecret();
        var code = ComputeCurrentCode(secret);

        Assert.True(sut.VerifyCode(secret, code));
        Assert.False(sut.VerifyCode(secret, "000000"));
    }

    private static string ComputeCurrentCode(string secretBase32)
    {
        // Aynı RFC uygulamasını doğrulamak için servisin ürettiği kodu bir adımda yeniden hesaplarız:
        // secret ile Verify true dönene kadar bilinen bir kod üretmek yerine, servis üzerinden
        // Generate + Verify round-trip: secret doğrulanabilir olmalı.
        // Burada HMAC'i Rfc6238TotpService ile aynı algoritmayla hesaplamak yerine,
        // GenerateSecret sonrası Verify'nin false-negatif olmadığını kontrol etmek için
        // window içindeki kodu Base32 decode + HMAC ile hesaplıyoruz.
        var key = Base32Encoding.ToBytes(secretBase32);
        var timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        return Compute(key, timestep);
    }

    private static string Compute(byte[] key, long timestep)
    {
        Span<byte> counter = stackalloc byte[8];
        for (var i = 7; i >= 0; i--)
        {
            counter[i] = (byte)(timestep & 0xff);
            timestep >>= 8;
        }

        Span<byte> hash = stackalloc byte[20];
        System.Security.Cryptography.HMACSHA1.HashData(key, counter, hash);
        var offset = hash[^1] & 0x0f;
        var binary =
            ((hash[offset] & 0x7f) << 24)
            | ((hash[offset + 1] & 0xff) << 16)
            | ((hash[offset + 2] & 0xff) << 8)
            | (hash[offset + 3] & 0xff);
        return (binary % 1_000_000).ToString("D6");
    }
}
