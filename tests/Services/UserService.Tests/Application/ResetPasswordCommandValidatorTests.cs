using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_token_and_strong_password()
    {
        var result = await _validator.ValidateAsync(
            new ResetPasswordCommand("tok", "Aa1!aaaa")
        );
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_token_with_validation_required()
    {
        var result = await _validator.ValidateAsync(
            new ResetPasswordCommand("", "Aa1!aaaa")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_token_over_max_length()
    {
        var tooLong = new string('a', ResetPasswordCommandValidator.TokenMaxLength + 1);
        var result = await _validator.ValidateAsync(
            new ResetPasswordCommand(tooLong, "Aa1!aaaa")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }

    [Fact]
    public async Task Rejects_weak_password()
    {
        var result = await _validator.ValidateAsync(
            new ResetPasswordCommand("tok", "weak")
        );
        Assert.False(result.IsValid);
    }
}
