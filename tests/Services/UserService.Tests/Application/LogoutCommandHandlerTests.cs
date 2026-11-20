using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class LogoutCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    [Fact]
    public async Task Fails_when_tenant_unresolved()
    {
        var handler = CreateHandler(tenantId: null, out _, out _);

        var result = await handler.Handle(new LogoutCommand("any-token"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
    }

    [Fact]
    public async Task Unknown_token_is_idempotent_success()
    {
        var handler = CreateHandler(TenantId, out var tokens, out var revocations);

        var result = await handler.Handle(new LogoutCommand("missing-token"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthLogoutSuccess, result.ErrorCode);
        Assert.Empty(tokens.Items);
        Assert.Empty(revocations.RevokedSessions);
    }

    [Fact]
    public async Task Revokes_family_refresh_rows_and_session_denylist()
    {
        var handler = CreateHandler(TenantId, out var tokens, out var revocations);
        var familyId = Guid.NewGuid();
        var (plaintext, entity) = RefreshTokenFactory.Create(UserId, familyId, 7, null);
        await tokens.AddAsync(entity, CancellationToken.None);
        var sibling = RefreshTokenFactory.Create(UserId, familyId, 7, null).Entity;
        await tokens.AddAsync(sibling, CancellationToken.None);
        var otherFamily = RefreshTokenFactory.Create(UserId, Guid.NewGuid(), 7, null).Entity;
        await tokens.AddAsync(otherFamily, CancellationToken.None);

        var result = await handler.Handle(new LogoutCommand(plaintext), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthLogoutSuccess, result.ErrorCode);
        Assert.True(entity.IsRevoked);
        Assert.True(sibling.IsRevoked);
        Assert.False(otherFamily.IsRevoked);
        var revoked = Assert.Single(revocations.RevokedSessions);
        Assert.Equal(familyId, revoked.SessionId);
    }

    [Fact]
    public async Task Already_revoked_token_still_writes_denylist()
    {
        var handler = CreateHandler(TenantId, out var tokens, out var revocations);
        var familyId = Guid.NewGuid();
        var (plaintext, entity) = RefreshTokenFactory.Create(UserId, familyId, 7, null);
        entity.Revoke();
        await tokens.AddAsync(entity, CancellationToken.None);

        var result = await handler.Handle(new LogoutCommand(plaintext), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthLogoutSuccess, result.ErrorCode);
        var revoked = Assert.Single(revocations.RevokedSessions);
        Assert.Equal(familyId, revoked.SessionId);
    }

    private static LogoutCommandHandler CreateHandler(
        Guid? tenantId,
        out InMemoryRepository<RefreshToken> tokens,
        out FakeAccessTokenRevocationStore revocations
    )
    {
        tokens = new InMemoryRepository<RefreshToken>();
        revocations = new FakeAccessTokenRevocationStore();
        var lifecycle = new AccessTokenLifecycle(
            new FakeUserAccessStampStore(),
            revocations,
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            },
            Options.Create(new AuthOptions { AccessTokenMinutes = 30 })
        );

        return new LogoutCommandHandler(
            tokens,
            lifecycle,
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            }
        );
    }
}
