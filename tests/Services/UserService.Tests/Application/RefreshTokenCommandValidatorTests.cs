using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_non_empty_token()
    {
        var result = await _validator.ValidateAsync(new RefreshTokenCommand("abc123"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_token_with_validation_required()
    {
        var result = await _validator.ValidateAsync(new RefreshTokenCommand(""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_token_over_max_length()
    {
        var tooLong = new string('a', RefreshTokenCommandValidator.RefreshTokenMaxLength + 1);
        var result = await _validator.ValidateAsync(new RefreshTokenCommand(tooLong));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }
}
