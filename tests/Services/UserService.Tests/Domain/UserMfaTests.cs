using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class UserMfaTests
{
    [Fact]
    public void BeginMfaEnrollment_then_EnableMfa_sets_flag()
    {
        var user = CreateUser();
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["HASH1", "HASH2"]);

        Assert.False(user.IsMfaEnabled);
        Assert.Equal("JBSWY3DPEHPK3PXP", user.TotpSecret);
        Assert.Equal(2, user.RecoveryCodeHashes.Count);

        user.EnableMfa();

        Assert.True(user.IsMfaEnabled);
    }

    [Fact]
    public void TryConsumeRecoveryCodeHash_removes_used_code()
    {
        var user = CreateUser();
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["AAA", "BBB"]);
        user.EnableMfa();

        Assert.True(user.TryConsumeRecoveryCodeHash("AAA"));
        Assert.False(user.TryConsumeRecoveryCodeHash("AAA"));
        Assert.Single(user.RecoveryCodeHashes);
        Assert.Contains("BBB", user.RecoveryCodeHashes);
    }

    [Fact]
    public void DisableMfa_clears_secret_and_codes()
    {
        var user = CreateUser();
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["AAA"]);
        user.EnableMfa();

        user.DisableMfa();

        Assert.False(user.IsMfaEnabled);
        Assert.Null(user.TotpSecret);
        Assert.Empty(user.RecoveryCodeHashes);
    }

    [Fact]
    public void BeginMfaEnrollment_throws_when_already_enabled()
    {
        var user = CreateUser();
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["AAA"]);
        user.EnableMfa();

        Assert.Throws<InvalidOperationException>(() =>
            user.BeginMfaEnrollment("OTHERSECRETKEY12", ["BBB"]));
    }

    private static User CreateUser() =>
        new("admin@omnisync.com", "admin", "hash", "System", "Admin");
}
