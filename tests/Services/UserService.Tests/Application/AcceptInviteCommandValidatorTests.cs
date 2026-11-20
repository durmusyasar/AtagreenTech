using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class AcceptInviteCommandValidatorTests
{
    private readonly AcceptInviteCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Accepts_valid_invite_payload()
    {
        var result = await _validator.ValidateAsync(
            new AcceptInviteCommand("tok", "Aa1!aaaa", "Ada", "Lovelace")
        );
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_token()
    {
        var result = await _validator.ValidateAsync(
            new AcceptInviteCommand("", "Aa1!aaaa", "Ada", "Lovelace")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationRequired);
    }

    [Fact]
    public async Task Rejects_weak_password()
    {
        var result = await _validator.ValidateAsync(
            new AcceptInviteCommand("tok", "weak", "Ada", "Lovelace")
        );
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Rejects_blank_names()
    {
        var result = await _validator.ValidateAsync(
            new AcceptInviteCommand("tok", "Aa1!aaaa", "", " ")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AcceptInviteCommand.FirstName));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AcceptInviteCommand.LastName));
    }

    [Fact]
    public async Task Rejects_name_over_max_length()
    {
        var tooLong = new string('A', User.NameMaxLength + 1);
        var result = await _validator.ValidateAsync(
            new AcceptInviteCommand("tok", "Aa1!aaaa", tooLong, "Lovelace")
        );
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == ErrorCodes.ValidationMaxLength);
    }
}
