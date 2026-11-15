using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class PasswordResetTests
{
    [Fact]
    public void ChangePassword_clears_reset_challenge_and_rotates_security_stamp()
    {
        var user = new User("user@example.com", "user", "hash-v1", "Ada", "Lovelace");
        var stampBefore = user.SecurityStamp;
        var (_, hash) = EmailVerificationTokenFactory.Create();
        user.SetPasswordResetChallenge(hash, DateTime.UtcNow.AddHours(1));

        user.ChangePassword("hash-v2");

        Assert.Equal("hash-v2", user.PasswordHash);
        Assert.NotEqual(stampBefore, user.SecurityStamp);
        Assert.Null(user.PasswordResetTokenHash);
        Assert.Null(user.PasswordResetTokenExpiresAt);
    }

    [Fact]
    public void Password_reset_challenge_expires()
    {
        var user = new User("user@example.com", "user", "hash", "Ada", "Lovelace");
        var (_, hash) = EmailVerificationTokenFactory.Create();
        user.SetPasswordResetChallenge(hash, DateTime.UtcNow.AddMinutes(1));

        Assert.True(user.IsPasswordResetChallengeValid(hash, DateTime.UtcNow));
        Assert.False(user.IsPasswordResetChallengeValid(hash, DateTime.UtcNow.AddHours(2)));
    }
}
