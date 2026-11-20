using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class EmailVerificationTokenFactoryTests
{
    [Fact]
    public void Hash_is_deterministic_sha256_hex()
    {
        var first = EmailVerificationTokenFactory.Hash("same-token");
        var second = EmailVerificationTokenFactory.Hash("same-token");

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
        Assert.Equal(first, first.ToUpperInvariant());
        Assert.NotEqual(first, EmailVerificationTokenFactory.Hash("other-token"));
    }

    [Fact]
    public void Hash_rejects_blank_with_validation_required()
    {
        var empty = Assert.Throws<ArgumentException>(() => EmailVerificationTokenFactory.Hash(""));
        Assert.StartsWith(ErrorCodes.ValidationRequired, empty.Message, StringComparison.Ordinal);
        Assert.Equal("plaintextToken", empty.ParamName);

        var whitespace = Assert.Throws<ArgumentException>(() =>
            EmailVerificationTokenFactory.Hash("   ")
        );
        Assert.StartsWith(ErrorCodes.ValidationRequired, whitespace.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_returns_matching_plaintext_and_hash()
    {
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();

        Assert.False(string.IsNullOrWhiteSpace(plaintext));
        Assert.Equal(EmailVerificationTokenFactory.Hash(plaintext), hash);
    }
}
