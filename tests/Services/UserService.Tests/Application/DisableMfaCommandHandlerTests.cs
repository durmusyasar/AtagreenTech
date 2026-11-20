using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class DisableMfaCommandHandlerTests
{
    private static readonly Guid UserId = Guid.Parse("dddddddd-4444-4444-4444-444444444444");
    private const string Password = "Aa1!aaaa";

    [Fact]
    public async Task Fails_when_unauthenticated()
    {
        var handler = CreateHandler(new FakeCurrentUser { IsAuthenticated = false }, out _, out _);

        var result = await handler.Handle(new DisableMfaCommand(Password), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthInvalidCredentials, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_password_mismatch()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out _);
        await users.AddAsync(CreateMfaEnabledUser(), CancellationToken.None);

        var result = await handler.Handle(new DisableMfaCommand("wrong"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthPasswordMismatch, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_mfa_already_disabled()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out _);
        var user = new User("a@b.com", "admin", "hashed:" + Password, "A", "B");
        SetUserId(user, UserId);
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new DisableMfaCommand(Password), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthMfaAlreadyDisabled, result.ErrorCode);
    }

    [Fact]
    public async Task Disables_mfa_on_valid_password()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out var audit);
        var user = CreateMfaEnabledUser();
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new DisableMfaCommand(Password), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthDisableMfaSuccess, result.ErrorCode);
        Assert.False(user.IsMfaEnabled);
        Assert.Null(user.TotpSecret);
        Assert.Contains(AuditLog.Actions.MfaDisabled, audit.Actions);
    }

    private static FakeCurrentUser AuthenticatedUser() =>
        new() { IsAuthenticated = true, UserId = UserId.ToString() };

    private static User CreateMfaEnabledUser()
    {
        var user = new User("a@b.com", "admin", "hashed:" + Password, "A", "B");
        SetUserId(user, UserId);
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["hash1"]);
        user.EnableMfa();
        return user;
    }

    private static DisableMfaCommandHandler CreateHandler(
        FakeCurrentUser currentUser,
        out InMemoryRepository<User> users,
        out FakeIdentityAuditLogger audit
    )
    {
        users = new InMemoryRepository<User>();
        audit = new FakeIdentityAuditLogger();
        return new DisableMfaCommandHandler(
            users,
            new FakePasswordHasher(),
            audit,
            currentUser,
            TestLocalizers.Passthrough
        );
    }

    private static void SetUserId(User user, Guid id) =>
        typeof(User).BaseType!.GetProperty("Id")!.SetValue(user, id);

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
