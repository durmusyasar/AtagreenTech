using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class VerifyMfaCommandValidatorTests
{
    private readonly VerifyMfaCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_valid_code()
    {
        var result = await _validator.ValidateAsync(new VerifyMfaCommand("123456"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_code()
    {
        var result = await _validator.ValidateAsync(new VerifyMfaCommand(""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_code_over_max_length()
    {
        var tooLong = new string('1', VerifyMfaCommandValidator.CodeMaxLength + 1);
        var result = await _validator.ValidateAsync(new VerifyMfaCommand(tooLong));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }
}
