using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_valid_email_and_password()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("admin@omnisync.com", "Admin123!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_email_with_validation_required()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("", "Admin123!"));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == ErrorCodes.ValidationRequired
        );
    }

    [Fact]
    public async Task Rejects_invalid_email_with_validation_invalid()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("not-an-email", "Admin123!"));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == ErrorCodes.ValidationInvalid
        );
    }

    [Fact]
    public async Task Rejects_empty_password_with_validation_required()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("admin@omnisync.com", ""));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == ErrorCodes.ValidationRequired
        );
    }

    [Fact]
    public async Task Rejects_password_over_max_length()
    {
        var tooLong = new string('a', PasswordRules.MaxLength + 1);
        var result = await _validator.ValidateAsync(new LoginCommand("admin@omnisync.com", tooLong));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == ErrorCodes.ValidationMaxLength
        );
    }

    [Fact]
    public async Task Rejects_email_over_max_length()
    {
        var local = new string('a', User.EmailMaxLength);
        var result = await _validator.ValidateAsync(
            new LoginCommand($"{local}@example.com", "Admin123!")
        );

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage == ErrorCodes.ValidationMaxLength
        );
    }
}
