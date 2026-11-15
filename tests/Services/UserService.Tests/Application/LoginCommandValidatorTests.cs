using AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Accepts_valid_email_and_password()
    {
        var result = await _validator.ValidateAsync(new LoginCommand("admin@omnisync.com", "Admin123!"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Admin123!")]
    [InlineData("not-an-email", "Admin123!")]
    [InlineData("admin@omnisync.com", "")]
    public async Task Rejects_invalid_payload(string email, string password)
    {
        var result = await _validator.ValidateAsync(new LoginCommand(email, password));

        Assert.False(result.IsValid);
    }
}
