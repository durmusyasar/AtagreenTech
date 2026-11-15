using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class UserLockoutTests
{
    [Fact]
    public void RegisterFailedAccess_locks_after_threshold_and_resets_counter()
    {
        var user = CreateUser();
        var now = new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < 4; i++)
            user.RegisterFailedAccess(5, TimeSpan.FromMinutes(15), now);

        Assert.Equal(4, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        Assert.False(user.IsLockedOut(now));

        user.RegisterFailedAccess(5, TimeSpan.FromMinutes(15), now);

        Assert.Equal(0, user.AccessFailedCount);
        Assert.Equal(now.AddMinutes(15), user.LockoutEnd);
        Assert.True(user.IsLockedOut(now));
        Assert.False(user.IsLockedOut(now.AddMinutes(16)));
    }

    [Fact]
    public void ResetAccessFailures_clears_lockout()
    {
        var user = CreateUser();
        var now = DateTime.UtcNow;
        for (var i = 0; i < 5; i++)
            user.RegisterFailedAccess(5, TimeSpan.FromMinutes(15), now);

        user.ResetAccessFailures();

        Assert.Equal(0, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        Assert.False(user.IsLockedOut(now));
    }

    private static User CreateUser() =>
        new("admin@omnisync.com", "admin", "hash", "System", "Admin");
}
