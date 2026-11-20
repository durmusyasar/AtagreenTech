using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class ResetPasswordCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    [Fact]
    public async Task Fails_when_tenant_unresolved()
    {
        var handler = CreateHandler(tenantId: null, out _, out _, out _, out _);

        var result = await handler.Handle(
            new ResetPasswordCommand("tok", "Aa1!aaaa"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_token_unknown()
    {
        var handler = CreateHandler(TenantId, out _, out _, out _, out _);

        var result = await handler.Handle(
            new ResetPasswordCommand("unknown", "Aa1!aaaa"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthResetTokenInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task Resets_password_revokes_sessions_and_audits()
    {
        var handler = CreateHandler(
            TenantId,
            out var users,
            out var refreshTokens,
            out var stamps,
            out var audit
        );
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        var user = new User("a@b.com", "admin", "old-hash", "A", "B");
        user.SetPasswordResetChallenge(hash, DateTime.UtcNow.AddHours(1));
        await users.AddAsync(user, CancellationToken.None);

        var familyId = Guid.NewGuid();
        await refreshTokens.AddAsync(
            RefreshTokenFactory.Create(user.Id, familyId, 7, null).Entity,
            CancellationToken.None
        );

        var result = await handler.Handle(
            new ResetPasswordCommand(plaintext, "Aa1!bbbb", "127.0.0.1"),
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthResetPasswordSuccess, result.ErrorCode);
        Assert.Equal("hashed:Aa1!bbbb", user.PasswordHash);
        Assert.Null(user.PasswordResetTokenHash);
        Assert.True(refreshTokens.Items.All(t => t.IsRevoked));
        Assert.Contains(stamps.Published, p => p.UserId == user.Id && p.Stamp == user.SecurityStamp);
        Assert.Contains(audit.Actions, a => a == AuditLog.Actions.PasswordReset);
    }

    private static ResetPasswordCommandHandler CreateHandler(
        Guid? tenantId,
        out InMemoryRepository<User> users,
        out InMemoryRepository<RefreshToken> refreshTokens,
        out FakeUserAccessStampStore stamps,
        out FakeIdentityAuditLogger audit
    )
    {
        users = new InMemoryRepository<User>();
        refreshTokens = new InMemoryRepository<RefreshToken>();
        stamps = new FakeUserAccessStampStore();
        audit = new FakeIdentityAuditLogger();
        var tenant = new FakeCurrentTenant
        {
            Id = tenantId,
            IsAvailable = tenantId is not null,
            Identifier = tenantId is null ? null : "atagreen",
        };
        var lifecycle = new AccessTokenLifecycle(
            stamps,
            new FakeAccessTokenRevocationStore(),
            tenant,
            Options.Create(new AuthOptions { AccessTokenMinutes = 30 })
        );

        return new ResetPasswordCommandHandler(
            users,
            refreshTokens,
            new FakePasswordHasher(),
            audit,
            lifecycle,
            tenant,
            TestLocalizers.Passthrough
        );
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => "hashed:" + password;

        public bool Verify(string password, string storedHash) => storedHash == Hash(password);
    }

    private sealed class FakeIdentityAuditLogger : IIdentityAuditLogger
    {
        public List<string> Actions { get; } = [];

        public Task LogAsync(
            string action,
            string email,
            Guid? userId = null,
            string? ipAddress = null,
            string? details = null,
            CancellationToken cancellationToken = default
        )
        {
            Actions.Add(action);
            return Task.CompletedTask;
        }
    }
}
