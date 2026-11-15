using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class UserInvitationTests
{
    [Fact]
    public void CreateInvited_is_inactive_until_accept()
    {
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        var user = User.CreateInvited(
            "viewer@example.com",
            "viewer@example.com",
            "pending-hash",
            "Vie",
            "Wer",
            hash,
            DateTime.UtcNow.AddDays(3)
        );

        Assert.False(user.IsActive);
        Assert.False(user.IsEmailVerified);
        Assert.True(user.IsInvitationChallengeValid(hash, DateTime.UtcNow));
        Assert.False(user.IsInvitationChallengeValid(EmailVerificationTokenFactory.Hash("other"), DateTime.UtcNow));

        user.AcceptInvitation("real-hash", "Viewer", "User");

        Assert.True(user.IsActive);
        Assert.True(user.IsEmailVerified);
        Assert.Equal("real-hash", user.PasswordHash);
        Assert.Equal("Viewer", user.FirstName);
        Assert.Null(user.InvitationTokenHash);
        Assert.Equal(EmailVerificationTokenFactory.Hash(plaintext), hash);
    }
}
