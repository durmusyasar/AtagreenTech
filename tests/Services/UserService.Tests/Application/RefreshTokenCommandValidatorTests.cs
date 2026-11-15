using AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public async Task Accepts_non_empty_token()
    {
        var result = await _validator.ValidateAsync(new RefreshTokenCommand("abc123"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Rejects_empty_token()
    {
        var result = await _validator.ValidateAsync(new RefreshTokenCommand(""));
        Assert.False(result.IsValid);
    }
}
