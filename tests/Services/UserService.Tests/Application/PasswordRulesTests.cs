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
        var (isValid, message) = PasswordRules.Validate(password);

        Assert.False(isValid);
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void Validate_accepts_strong_password()
    {
        var (isValid, message) = PasswordRules.Validate("Admin123!");

        Assert.True(isValid);
        Assert.Equal(string.Empty, message);
    }
}
