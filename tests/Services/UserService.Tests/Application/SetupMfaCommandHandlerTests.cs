using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.SetupMfa;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class SetupMfaCommandHandlerTests
{
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    [Fact]
    public async Task Fails_when_unauthenticated()
    {
        var handler = CreateHandler(new FakeCurrentUser { IsAuthenticated = false }, out _);

        var result = await handler.Handle(new SetupMfaCommand(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthInvalidCredentials, result.ErrorCode);
        Assert.Null(result.Setup);
    }

    [Fact]
    public async Task Fails_when_mfa_already_enabled()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users);
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        SetUserId(user, UserId);
        user.BeginMfaEnrollment("JBSWY3DPEHPK3PXP", ["hash1", "hash2"]);
        user.EnableMfa();
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new SetupMfaCommand(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthMfaAlreadyEnabled, result.ErrorCode);
    }

    [Fact]
    public async Task Starts_enrollment_and_returns_setup_payload()
    {
        var handler = CreateHandler(AuthenticatedUser(), out var users);
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        SetUserId(user, UserId);
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new SetupMfaCommand(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Setup);
        Assert.False(string.IsNullOrWhiteSpace(result.Setup.SharedKey));
        Assert.StartsWith("otpauth://totp/", result.Setup.QrCodeUri);
        Assert.NotEmpty(result.Setup.RecoveryCodes);
        Assert.False(user.IsMfaEnabled);
    }

    private static FakeCurrentUser AuthenticatedUser() =>
        new() { IsAuthenticated = true, UserId = UserId.ToString() };

    private static SetupMfaCommandHandler CreateHandler(
        FakeCurrentUser currentUser,
        out InMemoryRepository<User> users
    )
    {
        users = new InMemoryRepository<User>();
        return new SetupMfaCommandHandler(
            users,
            new FakeTotpService(),
            currentUser,
            Options.Create(
                new AuthOptions { MfaIssuer = "OmniSync", MfaRecoveryCodeCount = 8 }
            )
        );
    }

    private static void SetUserId(User user, Guid id) =>
        typeof(User).BaseType!.GetProperty("Id")!.SetValue(user, id);

    private sealed class FakeTotpService : ITotpService
    {
        public string GenerateSecret() => "JBSWY3DPEHPK3PXP";

        public string BuildAuthenticatorUri(string email, string secretBase32, string issuer) =>
            $"otpauth://totp/{issuer}:{email}?secret={secretBase32}&issuer={issuer}&digits=6";

        public bool VerifyCode(string secretBase32, string code) => code == "123456";
    }
}
