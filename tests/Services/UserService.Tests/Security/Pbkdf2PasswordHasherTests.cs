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
        Assert.Contains('.', hash);
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
        Assert.False(_hasher.Verify("", hash));
    }
}
