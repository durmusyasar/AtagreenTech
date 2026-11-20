using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class VerifyEmailCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    [Fact]
    public async Task Fails_when_tenant_unresolved()
    {
        var handler = CreateHandler(tenantId: null, out _);

        var result = await handler.Handle(new VerifyEmailCommand("any"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_token_unknown()
    {
        var handler = CreateHandler(TenantId, out _);

        var result = await handler.Handle(new VerifyEmailCommand("unknown-token"), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthVerifyTokenInvalid, result.ErrorCode);
    }

    [Fact]
    public async Task Fails_when_token_expired()
    {
        var handler = CreateHandler(TenantId, out var users);
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        // Geçmiş expiry DomainErrorCodes.ValidationInvalid fırlatır; challenge’ı geçerli yazıp süreyi bozmak için
        // valid future set + reflection yerine expired challenge: önce set, sonra alanları testte aşmak.
        user.SetEmailVerificationChallenge(hash, DateTime.UtcNow.AddHours(1));
        typeof(User)
            .GetProperty(nameof(User.EmailVerificationTokenExpiresAt))!
            .SetValue(user, DateTime.UtcNow.AddMinutes(-1));
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new VerifyEmailCommand(plaintext), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthVerifyTokenInvalid, result.ErrorCode);
        Assert.False(user.IsEmailVerified);
    }

    [Fact]
    public async Task Marks_email_verified_on_valid_token()
    {
        var handler = CreateHandler(TenantId, out var users);
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        user.SetEmailVerificationChallenge(hash, DateTime.UtcNow.AddHours(24));
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(new VerifyEmailCommand(plaintext), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthVerifyEmailSuccess, result.ErrorCode);
        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationTokenHash);
    }

    private static VerifyEmailCommandHandler CreateHandler(
        Guid? tenantId,
        out InMemoryRepository<User> users
    )
    {
        users = new InMemoryRepository<User>();
        return new VerifyEmailCommandHandler(
            users,
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            }
        );
    }
}
