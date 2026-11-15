using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class RefreshTokenReuseTests
{
    [Fact]
    public void Revoke_is_idempotent_and_marks_inactive()
    {
        var token = new RefreshToken(
            Guid.NewGuid(),
            "hash",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1)
        );

        token.Revoke();
        var revokedAt = token.RevokedAt;
        token.Revoke();

        Assert.True(token.IsRevoked);
        Assert.Equal(revokedAt, token.RevokedAt);
        Assert.False(token.IsActive(DateTime.UtcNow));
    }
}
