using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class AccessTokenLifecycleTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    [Fact]
    public void AccessTokenTtl_uses_default_when_minutes_invalid()
    {
        var lifecycle = CreateLifecycle(accessTokenMinutes: 0, tenantId: TenantId);
        Assert.Equal(TimeSpan.FromMinutes(JwtContract.DefaultAccessTokenMinutes), lifecycle.AccessTokenTtl);
    }

    [Fact]
    public async Task PublishStampAsync_publishes_to_current_tenant()
    {
        var stamps = new FakeUserAccessStampStore();
        var lifecycle = CreateLifecycle(30, TenantId, stamps: stamps);

        await lifecycle.PublishStampAsync(UserId, "  stamp-1  ", CancellationToken.None);

        var row = Assert.Single(stamps.Published);
        Assert.Equal(TenantId, row.TenantId);
        Assert.Equal(UserId, row.UserId);
        Assert.Equal("stamp-1", row.Stamp);
    }

    [Fact]
    public async Task PublishStampAsync_requires_resolved_tenant()
    {
        var lifecycle = CreateLifecycle(30, tenantId: null);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            lifecycle.PublishStampAsync(UserId, "stamp", CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, ex.Message);
    }

    [Fact]
    public async Task RevokeSessionAsync_records_denylist_with_access_ttl()
    {
        var revocations = new FakeAccessTokenRevocationStore();
        var lifecycle = CreateLifecycle(45, TenantId, revocations: revocations);
        var sessionId = Guid.NewGuid();

        await lifecycle.RevokeSessionAsync(sessionId, CancellationToken.None);

        var row = Assert.Single(revocations.RevokedSessions);
        Assert.Equal(sessionId, row.SessionId);
        Assert.Equal(TimeSpan.FromMinutes(45), row.Ttl);
    }

    [Fact]
    public async Task RevokeAllSessionsAsync_revokes_refresh_rows_and_families()
    {
        var revocations = new FakeAccessTokenRevocationStore();
        var lifecycle = CreateLifecycle(30, TenantId, revocations: revocations);
        var tokens = new InMemoryRepository<RefreshToken>();
        var familyA = Guid.NewGuid();
        var familyB = Guid.NewGuid();

        await tokens.AddAsync(
            RefreshTokenFactory.Create(UserId, familyA, 7, null).Entity,
            CancellationToken.None
        );
        await tokens.AddAsync(
            RefreshTokenFactory.Create(UserId, familyB, 7, null).Entity,
            CancellationToken.None
        );
        await tokens.AddAsync(
            RefreshTokenFactory.Create(Guid.NewGuid(), Guid.NewGuid(), 7, null).Entity,
            CancellationToken.None
        );

        await lifecycle.RevokeAllSessionsAsync(tokens, UserId, CancellationToken.None);

        Assert.Equal(2, tokens.Items.Count(t => t.UserId == UserId && t.IsRevoked));
        Assert.Contains(tokens.Items, t => t.UserId != UserId && !t.IsRevoked);
        Assert.Equal(2, revocations.RevokedSessions.Count);
        Assert.Contains(revocations.RevokedSessions, r => r.SessionId == familyA);
        Assert.Contains(revocations.RevokedSessions, r => r.SessionId == familyB);
    }

    [Fact]
    public async Task PublishStampAsync_rejects_empty_user_or_blank_stamp()
    {
        var lifecycle = CreateLifecycle(30, TenantId);

        var emptyUser = await Assert.ThrowsAsync<ArgumentException>(() =>
            lifecycle.PublishStampAsync(Guid.Empty, "stamp", CancellationToken.None)
        );
        AssertValidationRequired(emptyUser, "userId");

        var blankStamp = await Assert.ThrowsAsync<ArgumentException>(() =>
            lifecycle.PublishStampAsync(UserId, "  ", CancellationToken.None)
        );
        AssertValidationRequired(blankStamp, "securityStamp");
    }

    [Fact]
    public async Task RevokeSessionAsync_rejects_empty_session_id()
    {
        var lifecycle = CreateLifecycle(30, TenantId);
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            lifecycle.RevokeSessionAsync(Guid.Empty, CancellationToken.None)
        );
        AssertValidationRequired(ex, "sessionId");
    }

    [Fact]
    public async Task RevokeAllSessionsAsync_rejects_empty_user_id()
    {
        var lifecycle = CreateLifecycle(30, TenantId);
        var tokens = new InMemoryRepository<RefreshToken>();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            lifecycle.RevokeAllSessionsAsync(tokens, Guid.Empty, CancellationToken.None)
        );
        AssertValidationRequired(ex, "userId");
    }

    /// <summary>
    /// <see cref="ArgumentException.Message"/> BCL tarafından <c>(Parameter '…')</c> ekler; kod önekini doğrularız.
    /// </summary>
    private static void AssertValidationRequired(ArgumentException ex, string paramName)
    {
        Assert.StartsWith(ErrorCodes.ValidationRequired, ex.Message, StringComparison.Ordinal);
        Assert.Equal(paramName, ex.ParamName);
    }

    private static AccessTokenLifecycle CreateLifecycle(
        int accessTokenMinutes,
        Guid? tenantId,
        FakeUserAccessStampStore? stamps = null,
        FakeAccessTokenRevocationStore? revocations = null
    ) =>
        new(
            stamps ?? new FakeUserAccessStampStore(),
            revocations ?? new FakeAccessTokenRevocationStore(),
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            },
            Options.Create(new AuthOptions { AccessTokenMinutes = accessTokenMinutes })
        );
}
