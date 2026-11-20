using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_distinct_strong_new_password()
    {
        var result = await _validator.ValidateAsync(
            new ChangePasswordCommand("Old1!aaa", "New1!aaa")
        );
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_current_password()
    {
        var result = await _validator.ValidateAsync(
            new ChangePasswordCommand("", "New1!aaa")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_same_as_current_password()
    {
        var result = await _validator.ValidateAsync(
            new ChangePasswordCommand("Same1!aa", "Same1!aa")
        );
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == ErrorCodes.AuthPasswordSameAsCurrent
        );
    }

    [Fact]
    public async Task Rejects_weak_new_password()
    {
        var result = await _validator.ValidateAsync(
            new ChangePasswordCommand("Old1!aaa", "weak")
        );
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Rejects_current_password_over_max_length()
    {
        var tooLong = new string('a', PasswordRules.MaxLength + 1);
        var result = await _validator.ValidateAsync(
            new ChangePasswordCommand(tooLong, "New1!aaa")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }
}
