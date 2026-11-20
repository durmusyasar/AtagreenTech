using AtaGreenTech.SharedKernel.Security;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Tests.Fakes;

internal sealed class FakeCurrentTenant : ICurrentTenant
{
    public Guid? Id { get; init; }
    public string? Identifier { get; init; }
    public string? Name { get; init; }
    public string? ConnectionString { get; init; }
    public bool IsAvailable { get; init; }
}

internal sealed class FakeUserAccessStampStore : IUserAccessStampStore
{
    public List<(Guid TenantId, Guid UserId, string Stamp)> Published { get; } = [];

    public Task PublishAsync(
        Guid tenantId,
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken = default
    )
    {
        Published.Add((tenantId, userId, securityStamp));
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(
            Published.LastOrDefault(p => p.TenantId == tenantId && p.UserId == userId).Stamp
        );
}

internal sealed class FakeAccessTokenRevocationStore : IAccessTokenRevocationStore
{
    public List<(Guid SessionId, TimeSpan Ttl)> RevokedSessions { get; } = [];

    public Task RevokeSessionAsync(Guid sessionId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        RevokedSessions.Add((sessionId, ttl));
        return Task.CompletedTask;
    }

    public Task RevokeTokenAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult(RevokedSessions.Any(r => r.SessionId == sessionId));

    public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);
}
