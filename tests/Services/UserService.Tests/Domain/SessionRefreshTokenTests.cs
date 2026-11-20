using AtaGreenTech.UserService.Domain;
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

    [Fact]
    public void Ctor_rejects_empty_user_or_family_id()
    {
        var emptyUser = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(Guid.Empty, "hash", Guid.NewGuid(), DateTime.UtcNow.AddDays(1)));
        Assert.StartsWith(DomainErrorCodes.ValidationRequired, emptyUser.Message, StringComparison.Ordinal);
        Assert.Equal("userId", emptyUser.ParamName);

        var emptyFamily = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(Guid.NewGuid(), "hash", Guid.Empty, DateTime.UtcNow.AddDays(1)));
        Assert.StartsWith(DomainErrorCodes.ValidationRequired, emptyFamily.Message, StringComparison.Ordinal);
        Assert.Equal("familyId", emptyFamily.ParamName);
    }

    [Fact]
    public void Ctor_rejects_past_expiry()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new RefreshToken(Guid.NewGuid(), "hash", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-1)));
        Assert.StartsWith(DomainErrorCodes.ValidationInvalid, ex.Message, StringComparison.Ordinal);
        Assert.Equal("expiresAt", ex.ParamName);
    }
}
