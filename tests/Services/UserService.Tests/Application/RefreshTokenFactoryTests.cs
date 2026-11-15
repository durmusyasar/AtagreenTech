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
        Assert.NotEqual(first, RefreshTokenFactory.Hash("other-token"));
    }

    [Fact]
    public void Create_reuses_family_id_and_stores_hash_not_plaintext()
    {
        var familyId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var (plaintext, entity) = RefreshTokenFactory.Create(
            Guid.NewGuid(),
            familyId,
            refreshTokenDays: 7,
            clientIp: "127.0.0.1"
        );

        Assert.Equal(familyId, entity.FamilyId);
        Assert.Equal(RefreshTokenFactory.Hash(plaintext), entity.TokenHash);
        Assert.NotEqual(plaintext, entity.TokenHash);
        Assert.True(entity.IsActive(DateTime.UtcNow));
    }
}
