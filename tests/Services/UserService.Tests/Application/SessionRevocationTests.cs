using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class SessionRevocationTests
{
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    [Fact]
    public async Task Rejects_empty_user_id()
    {
        var tokens = new InMemoryRepository<RefreshToken>();
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            SessionRevocation.RevokeAllRefreshTokensAsync(tokens, Guid.Empty, CancellationToken.None)
        );
        Assert.StartsWith(ErrorCodes.ValidationRequired, ex.Message, StringComparison.Ordinal);
        Assert.Equal("userId", ex.ParamName);
    }

    [Fact]
    public async Task Without_lifecycle_revokes_refresh_rows_only()
    {
        var tokens = new InMemoryRepository<RefreshToken>();
        var familyA = Guid.NewGuid();
        await tokens.AddAsync(RefreshTokenFactory.Create(UserId, familyA, 7, null).Entity);
        await tokens.AddAsync(
            RefreshTokenFactory.Create(Guid.NewGuid(), Guid.NewGuid(), 7, null).Entity
        );

        await SessionRevocation.RevokeAllRefreshTokensAsync(tokens, UserId, CancellationToken.None);

        Assert.True(tokens.Items.Where(t => t.UserId == UserId).All(t => t.IsRevoked));
        Assert.Contains(tokens.Items, t => t.UserId != UserId && !t.IsRevoked);
    }

    [Fact]
    public async Task With_lifecycle_also_writes_session_denylist()
    {
        var tokens = new InMemoryRepository<RefreshToken>();
        var revocations = new FakeAccessTokenRevocationStore();
        var familyId = Guid.NewGuid();
        await tokens.AddAsync(RefreshTokenFactory.Create(UserId, familyId, 7, null).Entity);
        var lifecycle = new AccessTokenLifecycle(
            new FakeUserAccessStampStore(),
            revocations,
            new FakeCurrentTenant
            {
                Id = TenantId,
                IsAvailable = true,
                Identifier = "atagreen",
            },
            Options.Create(new AuthOptions { AccessTokenMinutes = 30 })
        );

        await SessionRevocation.RevokeAllRefreshTokensAsync(
            tokens,
            UserId,
            CancellationToken.None,
            lifecycle
        );

        Assert.True(tokens.Items.Single().IsRevoked);
        Assert.Contains(revocations.RevokedSessions, r => r.SessionId == familyId);
    }
}
