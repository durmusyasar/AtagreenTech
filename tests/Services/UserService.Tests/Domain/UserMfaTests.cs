using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain;
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

        var ex = Assert.Throws<InvalidOperationException>(() =>
            user.BeginMfaEnrollment("OTHERSECRETKEY12", ["BBB"]));
        Assert.Equal(DomainErrorCodes.AuthMfaAlreadyEnabled, ex.Message);
        Assert.Equal(ErrorCodes.AuthMfaAlreadyEnabled, DomainErrorCodes.AuthMfaAlreadyEnabled);
    }

    [Fact]
    public void EnableMfa_throws_when_enrollment_missing()
    {
        var user = CreateUser();
        var ex = Assert.Throws<InvalidOperationException>(() => user.EnableMfa());
        Assert.Equal(DomainErrorCodes.AuthMfaSetupRequired, ex.Message);
    }

    [Fact]
    public void SetPasswordResetChallenge_rejects_past_expiry()
    {
        var user = CreateUser();
        var ex = Assert.Throws<ArgumentException>(() =>
            user.SetPasswordResetChallenge("hash", DateTime.UtcNow.AddMinutes(-1)));
        Assert.StartsWith(DomainErrorCodes.ValidationInvalid, ex.Message, StringComparison.Ordinal);
        Assert.Equal("expiresAtUtc", ex.ParamName);
    }

    [Fact]
    public void Ctor_rejects_email_over_max_length()
    {
        var tooLong = new string('a', User.EmailMaxLength + 1) + "@x.com";
        var ex = Assert.Throws<ArgumentException>(() =>
            new User(tooLong, "admin", "hash", "System", "Admin"));
        Assert.StartsWith(DomainErrorCodes.ValidationMaxLength, ex.Message, StringComparison.Ordinal);
        Assert.Equal("email", ex.ParamName);
    }

    private static User CreateUser() =>
        new("admin@omnisync.com", "admin", "hash", "System", "Admin");
}
