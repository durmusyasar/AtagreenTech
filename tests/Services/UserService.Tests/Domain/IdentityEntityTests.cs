using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class IdentityEntityTests
{
    [Fact]
    public void User_normalizes_email_and_rotates_security_stamp_on_password_change()
    {
        var user = new User("Admin@Example.COM", "admin", "hash-v1", "Ada", "Lovelace");

        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal("admin", user.Username);
        Assert.False(string.IsNullOrWhiteSpace(user.SecurityStamp));

        var stampBefore = user.SecurityStamp;
        user.ChangePassword("hash-v2");

        Assert.Equal("hash-v2", user.PasswordHash);
        Assert.NotEqual(stampBefore, user.SecurityStamp);
        Assert.DoesNotContain("TenantId", typeof(User).GetProperties().Select(p => p.Name));
    }

    [Fact]
    public void RefreshToken_stores_hash_and_supports_revoke()
    {
        var familyId = Guid.NewGuid();
        var token = new RefreshToken(
            Guid.NewGuid(),
            "token-hash",
            familyId,
            DateTime.UtcNow.AddDays(7),
            "127.0.0.1"
        );

        Assert.Equal("token-hash", token.TokenHash);
        Assert.Equal(familyId, token.FamilyId);
        Assert.True(token.IsActive(DateTime.UtcNow));
        Assert.Null(typeof(RefreshToken).GetProperty("Token"));

        token.Revoke();

        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedAt);
        Assert.False(token.IsActive(DateTime.UtcNow));
    }

    [Fact]
    public void Role_exposes_standard_names()
    {
        var role = new Role(Role.Names.Admin, "Tenant yöneticisi");

        Assert.Equal(Role.Names.Admin, role.Name);
        Assert.Equal("Admin", Role.Names.Admin);
        Assert.Equal("Manager", Role.Names.Manager);
        Assert.Equal("Viewer", Role.Names.Viewer);
    }
}
