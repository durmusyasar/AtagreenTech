using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class SessionRefreshTokenTests
{
    [Fact]
    public void Stores_device_info_and_truncates_when_too_long()
    {
        var longAgent = new string('A', RefreshToken.DeviceInfoMaxLength + 50);
        var token = new RefreshToken(
            Guid.NewGuid(),
            "hash",
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1),
            "127.0.0.1",
            longAgent
        );

        Assert.Equal("127.0.0.1", token.CreatedByIp);
        Assert.Equal(RefreshToken.DeviceInfoMaxLength, token.DeviceInfo!.Length);
        Assert.StartsWith("AAA", token.DeviceInfo);
    }
}
