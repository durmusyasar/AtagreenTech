using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class EmailVerificationTests
{
    [Fact]
    public void Challenge_roundtrip_verifies_and_clears_token()
    {
        var user = new User("user@example.com", "user", "hash", "Ada", "Lovelace");
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();

        user.SetEmailVerificationChallenge(hash, DateTime.UtcNow.AddHours(24));

        Assert.False(user.IsEmailVerified);
        Assert.True(user.IsEmailVerificationChallengeValid(hash, DateTime.UtcNow));
        Assert.False(user.IsEmailVerificationChallengeValid(EmailVerificationTokenFactory.Hash("other"), DateTime.UtcNow));

        user.MarkEmailVerified();

        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationTokenHash);
        Assert.Null(user.EmailVerificationTokenExpiresAt);
        Assert.Equal(EmailVerificationTokenFactory.Hash(plaintext), hash);
    }

    [Fact]
    public void Expired_challenge_is_invalid()
    {
        var user = new User("user@example.com", "user", "hash", "Ada", "Lovelace");
        var (_, hash) = EmailVerificationTokenFactory.Create();
        user.SetEmailVerificationChallenge(hash, DateTime.UtcNow.AddMinutes(1));

        Assert.False(user.IsEmailVerificationChallengeValid(hash, DateTime.UtcNow.AddHours(2)));
    }
}
