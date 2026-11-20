using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class RefreshTokenFactoryTests
{
    [Fact]
    public void Hash_is_deterministic_sha256_hex()
    {
        var first = RefreshTokenFactory.Hash("same-token");
        var second = RefreshTokenFactory.Hash("same-token");

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
        Assert.Equal(first, first.ToUpperInvariant());
        Assert.NotEqual(first, RefreshTokenFactory.Hash("other-token"));
    }

    [Fact]
    public void Hash_rejects_null_empty_or_whitespace()
    {
        Assert.ThrowsAny<ArgumentException>(() => RefreshTokenFactory.Hash(null!));
        Assert.ThrowsAny<ArgumentException>(() => RefreshTokenFactory.Hash(""));
        Assert.ThrowsAny<ArgumentException>(() => RefreshTokenFactory.Hash("   "));
    }

    [Fact]
    public void Create_reuses_family_id_and_stores_hash_not_plaintext()
    {
        var familyId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var userId = Guid.NewGuid();
        var (plaintext, entity) = RefreshTokenFactory.Create(
            userId,
            familyId,
            refreshTokenDays: 7,
            clientIp: "127.0.0.1",
            deviceInfo: "TestAgent"
        );

        Assert.Equal(userId, entity.UserId);
        Assert.Equal(familyId, entity.FamilyId);
        Assert.Equal(RefreshTokenFactory.Hash(plaintext), entity.TokenHash);
        Assert.NotEqual(plaintext, entity.TokenHash);
        Assert.Equal("127.0.0.1", entity.CreatedByIp);
        Assert.Equal("TestAgent", entity.DeviceInfo);
        Assert.True(entity.IsActive(DateTime.UtcNow));
        Assert.True(entity.ExpiresAt > DateTime.UtcNow.AddDays(6));
        Assert.True(entity.ExpiresAt <= DateTime.UtcNow.AddDays(8));
    }

    [Fact]
    public void Create_produces_unique_plaintexts()
    {
        var userId = Guid.NewGuid();
        var familyId = Guid.NewGuid();
        var first = RefreshTokenFactory.Create(userId, familyId, 7, null);
        var second = RefreshTokenFactory.Create(userId, familyId, 7, null);

        Assert.NotEqual(first.Plaintext, second.Plaintext);
        Assert.NotEqual(first.Entity.TokenHash, second.Entity.TokenHash);
        Assert.Equal(familyId, first.Entity.FamilyId);
        Assert.Equal(familyId, second.Entity.FamilyId);
    }

    [Theory]
    [InlineData(true, false, 7)]
    [InlineData(false, true, 7)]
    [InlineData(false, false, 0)]
    [InlineData(false, false, -1)]
    public void Create_rejects_invalid_identity_or_lifetime(bool emptyUser, bool emptyFamily, int days)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            RefreshTokenFactory.Create(
                emptyUser ? Guid.Empty : Guid.NewGuid(),
                emptyFamily ? Guid.Empty : Guid.NewGuid(),
                days,
                null
            )
        );
    }
}
