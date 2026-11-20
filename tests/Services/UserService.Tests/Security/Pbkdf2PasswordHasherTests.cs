using System.Security.Cryptography;
using System.Text;
using AtaGreenTech.UserService.Infrastructure.Security;

namespace AtaGreenTech.UserService.Tests.Security;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_then_Verify_succeeds_for_same_password()
    {
        var hash = _hasher.Hash("Str0ng-P@ss!");

        Assert.True(_hasher.Verify("Str0ng-P@ss!", hash));
        Assert.StartsWith("v1.", hash);
        Assert.Equal(3, hash.Split('.').Length);
    }

    [Fact]
    public void Hash_produces_unique_salts()
    {
        var first = _hasher.Hash("same-password");
        var second = _hasher.Hash("same-password");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.Verify("same-password", first));
        Assert.True(_hasher.Verify("same-password", second));
    }

    [Fact]
    public void Verify_rejects_wrong_password_and_malformed_hash()
    {
        var hash = _hasher.Hash("correct");

        Assert.False(_hasher.Verify("wrong", hash));
        Assert.False(_hasher.Verify("correct", "not-a-hash"));
        Assert.False(_hasher.Verify("correct", "abc.def"));
        Assert.False(_hasher.Verify("correct", "v1.abc.def"));
        Assert.False(_hasher.Verify("", hash));
    }

    [Fact]
    public void Hash_throws_on_null_or_empty_password()
    {
        Assert.Throws<ArgumentNullException>(() => _hasher.Hash(null!));
        Assert.Throws<ArgumentException>(() => _hasher.Hash(""));
    }

    [Fact]
    public void Verify_accepts_legacy_salt_hash_format()
    {
        const string password = "LegacyPass1!";
        var salt = RandomNumberGenerator.GetBytes(16);
        var derived = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32
        );
        var legacy = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(derived)}";

        Assert.True(_hasher.Verify(password, legacy));
        Assert.False(_hasher.Verify("wrong", legacy));
    }

    [Fact]
    public void Verify_rejects_wrong_length_payload()
    {
        var shortSalt = Convert.ToBase64String(new byte[8]);
        var hash32 = Convert.ToBase64String(new byte[32]);
        Assert.False(_hasher.Verify("any", $"v1.{shortSalt}.{hash32}"));

        var salt16 = Convert.ToBase64String(new byte[16]);
        var shortHash = Convert.ToBase64String(new byte[16]);
        Assert.False(_hasher.Verify("any", $"v1.{salt16}.{shortHash}"));
    }
}
