using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class PasswordRulesTests
{
    [Theory]
    [InlineData(null, ErrorCodes.ValidationPasswordMinLength)]
    [InlineData("", ErrorCodes.ValidationPasswordMinLength)]
    [InlineData("   ", ErrorCodes.ValidationPasswordMinLength)]
    [InlineData("Short1!", ErrorCodes.ValidationPasswordMinLength)]
    [InlineData("nouppercase1!", ErrorCodes.ValidationPasswordUpper)]
    [InlineData("NOLOWERCASE1!", ErrorCodes.ValidationPasswordLower)]
    [InlineData("NoDigits!!", ErrorCodes.ValidationPasswordDigit)]
    [InlineData("NoSpecial1", ErrorCodes.ValidationPasswordSpecial)]
    public void Validate_rejects_weak_passwords_with_expected_error_code(string? password, string expectedErrorCode)
    {
        var (isValid, errorCode, _) = PasswordRules.Validate(password);

        Assert.False(isValid);
        Assert.Equal(expectedErrorCode, errorCode);
    }

    [Fact]
    public void Validate_rejects_password_exceeding_max_length()
    {
        var tooLong = "Aa1!" + new string('x', PasswordRules.MaxLength);

        var (isValid, errorCode, args) = PasswordRules.Validate(tooLong);

        Assert.False(isValid);
        Assert.Equal(ErrorCodes.ValidationPasswordMaxLength, errorCode);
        Assert.Equal([PasswordRules.MaxLength], args);
    }

    [Fact]
    public void Validate_accepts_strong_password()
    {
        var (isValid, errorCode, args) = PasswordRules.Validate("Admin123!");

        Assert.True(isValid);
        Assert.Equal(string.Empty, errorCode);
        Assert.Empty(args);
    }

    [Fact]
    public void Validate_requires_ascii_upper_even_when_unicode_letters_present()
    {
        // Unicode harfler (ş/İ) ASCII [A-Z]/[a-z] sayılmaz — FE passwordPolicy ile aynı.
        var (isValid, errorCode, _) = PasswordRules.Validate("şifreİ123!");

        Assert.False(isValid);
        Assert.Equal(ErrorCodes.ValidationPasswordUpper, errorCode);
    }
}
