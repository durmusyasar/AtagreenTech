using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class VerifyMfaCommandHandlerTests
{
    private static readonly Guid UserId = Guid.Parse("cccccccc-3333-3333-3333-333333333333");

    [Fact]
    public async Task Fails_when_unauthenticated()
    {
        var handler = CreateHandler(new FakeCurrentUser { IsAuthenticated = false }, out _, out _);

        var result = await handler.Handle(new VerifyMfaCommand("123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthInvalidCredentials, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_mfa_already_enabled()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out _);
        var user = CreateEnrolledUser();
        user.EnableMfa();
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new VerifyMfaCommand("123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthMfaAlreadyEnabled, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_setup_not_started()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out _);
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        SetUserId(user, UserId);
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new VerifyMfaCommand("123456"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthMfaSetupRequired, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_code_invalid()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out _);
        await users.AddAsync(CreateEnrolledUser(), CancellationToken.None);

        var result = await handler.Handle(new VerifyMfaCommand("000000"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthMfaInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task Enables_mfa_on_valid_code()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users, out var audit);
        var user = CreateEnrolledUser();
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new VerifyMfaCommand("123456"), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthVerifyMfaSuccess, result.ErrorCode);
        Assert.True(user.IsMfaEnabled);
        Assert.Contains(AuditLog.Actions.MfaEnabled, audit.Actions);
    }

    private static FakeCurrentUser AuthenticatedUser() =>
        new() { IsAuthenticated = true, UserId = UserId.ToString() };

    private static User CreateEnrolledUser()
    {
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        SetUserId(user, UserId);
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["hash1"]);
        return user;
    }

    private static VerifyMfaCommandHandler CreateHandler(
        FakeCurrentUser currentUser,
        out InMemoryRepository<User> users,
        out FakeIdentityAuditLogger audit
    )
    {
        users = new InMemoryRepository<User>();
        audit = new FakeIdentityAuditLogger();
        return new VerifyMfaCommandHandler(
            users,
            new FakeTotpService(),
            audit,
            currentUser,
            TestLocalizers.Passthrough
        );
    }

    private static void SetUserId(User user, Guid id) =>
        typeof(User).BaseType!.GetProperty("Id")!.SetValue(user, id);

    private sealed class FakeTotpService : ITotpService
    {
        public string GenerateSecret() => "JBSWY3DPEHPK3PXP";

        public string BuildAuthenticatorUri(string email, string secretBase32, string issuer) =>
            $"otpauth://totp/{issuer}:{email}?secret={secretBase32}";

        public bool VerifyCode(string secretBase32, string code) => code == "123456";
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
