using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class ChangePasswordCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    [Fact]
    public async Task Fails_when_unauthenticated()
    {
        var handler = CreateHandler(
            new FakeCurrentUser { IsAuthenticated = false },
            TenantId,
            out _,
            out _
        );

        var result = await handler.Handle(
            new ChangePasswordCommand("Old1!aaa", "New1!aaa"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthInvalidCredentials, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_tenant_unresolved()
    {
        var handler = CreateHandler(
            AuthenticatedUser(),
            tenantId: null,
            out _,
            out _
        );

        var result = await handler.Handle(
            new ChangePasswordCommand("Old1!aaa", "New1!aaa"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_with_password_mismatch_without_401_semantics()
    {
        var handler = CreateHandler(AuthenticatedUser(), TenantId, out var users, out _);
        var user = new User("a@b.com", "admin", "hashed:Old1!aaa", "A", "B");
        SetUserId(user, UserId);
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(
            new ChangePasswordCommand("Wrong1!aa", "New1!aaa"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthPasswordMismatch, result.ErrorCode);
        Assert.Equal("hashed:Old1!aaa", user.PasswordHash);
    }

    [Fact]
    public async Task Changes_password_and_revokes_sessions()
    {
        var handler = CreateHandler(
            AuthenticatedUser(),
            TenantId,
            out var users,
            out var refreshTokens
        );
        var user = new User("a@b.com", "admin", "hashed:Old1!aaa", "A", "B");
        SetUserId(user, UserId);
        await users.AddAsync(user, CancellationToken.None);
        await refreshTokens.AddAsync(
            RefreshTokenFactory.Create(UserId, Guid.NewGuid(), 7, null).Entity,
            CancellationToken.None
        );

        var result = await handler.Handle(
            new ChangePasswordCommand("Old1!aaa", "New1!bbb"),
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthChangePasswordSuccess, result.ErrorCode);
        Assert.Equal("hashed:New1!bbb", user.PasswordHash);
        Assert.True(refreshTokens.Items.All(t => t.IsRevoked));
    }

    private static FakeCurrentUser AuthenticatedUser() =>
        new() { IsAuthenticated = true, UserId = UserId.ToString() };

    private static ChangePasswordCommandHandler CreateHandler(
        FakeCurrentUser currentUser,
        Guid? tenantId,
        out InMemoryRepository<User> users,
        out InMemoryRepository<RefreshToken> refreshTokens
    )
    {
        users = new InMemoryRepository<User>();
        refreshTokens = new InMemoryRepository<RefreshToken>();
        var tenant = new FakeCurrentTenant
        {
            Id = tenantId,
            IsAvailable = tenantId is not null,
            Identifier = tenantId is null ? null : "atagreen",
        };
        var lifecycle = new AccessTokenLifecycle(
            new FakeUserAccessStampStore(),
            new FakeAccessTokenRevocationStore(),
            tenant,
            Options.Create(new AuthOptions { AccessTokenMinutes = 30 })
        );

        return new ChangePasswordCommandHandler(
            users,
            refreshTokens,
            new FakePasswordHasher(),
            currentUser,
            tenant,
            lifecycle
        );
    }

    private static void SetUserId(User user, Guid id) =>
        typeof(User).BaseType!.GetProperty("Id")!.SetValue(user, id);

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => "hashed:" + password;

        public bool Verify(string password, string storedHash) => storedHash == Hash(password);
    }
}
