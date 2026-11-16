using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class RecoveryCodeFactoryTests
{
    [Fact]
    public void Create_returns_unique_codes_and_matching_hashes()
    {
        var (plaintext, hashes) = RecoveryCodeFactory.Create(8);

        Assert.Equal(8, plaintext.Count);
        Assert.Equal(8, hashes.Count);
        Assert.Equal(8, plaintext.Distinct(StringComparer.Ordinal).Count());

        for (var i = 0; i < plaintext.Count; i++)
            Assert.Equal(RecoveryCodeFactory.Hash(plaintext[i]), hashes[i]);
    }

    [Fact]
    public void Hash_is_case_insensitive_after_normalize()
    {
        var hash1 = RecoveryCodeFactory.Hash("ab12cd34");
        var hash2 = RecoveryCodeFactory.Hash("AB12CD34");

        Assert.Equal(hash1, hash2);
    }
}
