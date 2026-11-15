using AtaGreenTech.UserService.Application.Features.Users;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class UserManagementHelpersTests
{
    [Fact]
    public void ResolveUsername_uses_email_when_within_limit()
    {
        var email = "viewer@example.com";
        Assert.Equal(email, UserManagementHelpers.ResolveUsername(email));
    }

    [Fact]
    public void ResolveUsername_truncates_local_part_when_email_too_long()
    {
        var local = new string('a', User.UsernameMaxLength + 10);
        var email = $"{local}@example.com";

        var username = UserManagementHelpers.ResolveUsername(email);

        Assert.Equal(User.UsernameMaxLength, username.Length);
        Assert.Equal(local[..User.UsernameMaxLength], username);
    }

    [Fact]
    public void NormalizeRoleNames_dedupes_case_insensitively()
    {
        var names = UserManagementHelpers.NormalizeRoleNames(["Admin", "admin", " Viewer "]);

        Assert.Equal(2, names.Count);
        Assert.Contains("Admin", names);
        Assert.Contains("Viewer", names);
    }
}
