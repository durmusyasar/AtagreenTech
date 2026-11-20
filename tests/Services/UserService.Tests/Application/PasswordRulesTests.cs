using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class PasswordRulesTests
{
    [Theory]
    [InlineData("Short1!")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigits!!")]
    [InlineData("NoSpecial1")]
    public void Validate_rejects_weak_passwords(string password)
    {
        var (isValid, errorCode, _) = PasswordRules.Validate(password);

        Assert.False(isValid);
        Assert.False(string.IsNullOrWhiteSpace(errorCode));
    }

    [Fact]
    public void Validate_accepts_strong_password()
    {
        var (isValid, errorCode, _) = PasswordRules.Validate("Admin123!");

        Assert.True(isValid);
        Assert.Equal(string.Empty, errorCode);
    }
}
