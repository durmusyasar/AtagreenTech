using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Infrastructure.Security;

internal sealed class IdentityAuditLogger(IRepository<AuditLog> auditLogs) : IIdentityAuditLogger
{
    public Task LogAsync(
        string action,
        string email,
        Guid? userId = null,
        string? ipAddress = null,
        string? details = null,
        CancellationToken cancellationToken = default
    ) =>
        auditLogs.AddAsync(
            new AuditLog(action, email, userId, ipAddress, details),
            cancellationToken
        );
}
