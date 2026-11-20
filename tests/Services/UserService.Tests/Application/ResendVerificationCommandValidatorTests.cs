using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class ResendVerificationCommandValidatorTests
{
    private readonly ResendVerificationCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_valid_email()
    {
        var result = await _validator.ValidateAsync(new ResendVerificationCommand("a@b.com"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_email_with_validation_required()
    {
        var result = await _validator.ValidateAsync(new ResendVerificationCommand(""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_invalid_email_format()
    {
        var result = await _validator.ValidateAsync(new ResendVerificationCommand("not-an-email"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationInvalid);
    }

    [Fact]
    public async Task Rejects_email_over_max_length()
    {
        var tooLong = new string('a', User.EmailMaxLength) + "@b.com";
        var result = await _validator.ValidateAsync(new ResendVerificationCommand(tooLong));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }
}
