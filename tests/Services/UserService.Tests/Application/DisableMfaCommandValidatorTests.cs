using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class DisableMfaCommandValidatorTests
{
    private readonly DisableMfaCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_non_empty_password()
    {
        var result = await _validator.ValidateAsync(new DisableMfaCommand("Aa1!aaaa"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_password()
    {
        var result = await _validator.ValidateAsync(new DisableMfaCommand(""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_password_over_max_length()
    {
        var tooLong = new string('a', PasswordRules.MaxLength + 1);
        var result = await _validator.ValidateAsync(new DisableMfaCommand(tooLong));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }
}
