using AtaGreenTech.UserService.Application.Features.Users.Commands.CreateUser;
using AtaGreenTech.UserService.Tests;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new(TestLocalizers.Passthrough);

    [Fact]
    public async Task Valid_command_passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateUserCommand(
                "user@example.com",
                "Password1!",
                "Ada",
                "Lovelace",
                ["Viewer"],
                true
            )
        );

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Empty_roles_fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateUserCommand("user@example.com", "Password1!", "Ada", "Lovelace", [], true)
        );

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Roles");
    }
}
