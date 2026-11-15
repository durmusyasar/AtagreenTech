using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class AuditLogTests
{
    [Fact]
    public void Constructor_normalizes_action_and_email()
    {
        var log = new AuditLog(
            " login_success ",
            "Admin@OmniSync.Com",
            Guid.NewGuid(),
            " 127.0.0.1 ",
            " ok "
        );

        Assert.Equal(AuditLog.Actions.LoginSuccess, log.Action);
        Assert.Equal("admin@omnisync.com", log.Email);
        Assert.Equal("127.0.0.1", log.IpAddress);
        Assert.Equal("ok", log.Details);
    }

    [Fact]
    public void Constructor_truncates_long_details()
    {
        var details = new string('x', AuditLog.DetailsMaxLength + 50);
        var log = new AuditLog(AuditLog.Actions.LoginFailure, "a@b.com", details: details);

        Assert.Equal(AuditLog.DetailsMaxLength, log.Details!.Length);
    }
}
