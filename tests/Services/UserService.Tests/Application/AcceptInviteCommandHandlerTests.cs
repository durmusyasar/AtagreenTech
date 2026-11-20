using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class AcceptInviteCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    [Fact]
    public async Task Fails_when_tenant_unresolved()
    {
        var handler = CreateHandler(tenantId: null, out _);

        var result = await handler.Handle(
            new AcceptInviteCommand("tok", "Aa1!aaaa", "Ada", "Lovelace"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_invite_token_unknown()
    {
        var handler = CreateHandler(TenantId, out _);

        var result = await handler.Handle(
            new AcceptInviteCommand("unknown", "Aa1!aaaa", "Ada", "Lovelace"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthInviteTokenInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task Accepts_valid_invitation()
    {
        var handler = CreateHandler(TenantId, out var users);
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        var user = User.CreateInvited(
            "invitee@b.com",
            "invitee",
            pendingPasswordHash: "pending-hash",
            firstName: "Ada",
            lastName: "Lovelace",
            invitationTokenHash: hash,
            invitationExpiresAtUtc: DateTime.UtcNow.AddDays(3)
        );
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(
            new AcceptInviteCommand(plaintext, "Aa1!bbbb", "Ada", "Byron"),
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthAcceptInviteSuccess, result.ErrorCode);
        Assert.True(user.IsEmailVerified);
        Assert.True(user.IsActive);
        Assert.Null(user.InvitationTokenHash);
        Assert.Equal("hashed:Aa1!bbbb", user.PasswordHash);
        Assert.Equal("Byron", user.LastName);
    }

    private static AcceptInviteCommandHandler CreateHandler(
        Guid? tenantId,
        out InMemoryRepository<User> users
    )
    {
        users = new InMemoryRepository<User>();
        return new AcceptInviteCommandHandler(
            users,
            new FakePasswordHasher(),
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            }
        );
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => "hashed:" + password;

        public bool Verify(string password, string storedHash) => storedHash == Hash(password);
    }
}
