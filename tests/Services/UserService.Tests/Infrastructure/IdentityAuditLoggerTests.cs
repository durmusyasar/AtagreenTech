using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Infrastructure.Security;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Infrastructure;

public sealed class IdentityAuditLoggerTests
{
    [Fact]
    public async Task LogAsync_appends_normalized_audit_row_without_save_changes()
    {
        var store = new InMemoryRepository<AuditLog>();
        var logger = new IdentityAuditLogger(store);
        var userId = Guid.NewGuid();

        await logger.LogAsync(
            AuditLog.Actions.LoginSuccess,
            "Admin@Example.COM",
            userId,
            " 127.0.0.1 ",
            "  ok  ",
            CancellationToken.None
        );

        var row = Assert.Single(store.Items);
        Assert.Equal(AuditLog.Actions.LoginSuccess, row.Action);
        Assert.Equal("admin@example.com", row.Email);
        Assert.Equal(userId, row.UserId);
        Assert.Equal("127.0.0.1", row.IpAddress);
        Assert.Equal("ok", row.Details);
    }
}
