using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class ResendVerificationCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    [Fact]
    public async Task Fails_when_tenant_unresolved()
    {
        var handler = CreateHandler(tenantId: null, out _, out _);

        var result = await handler.Handle(
            new ResendVerificationCommand("a@b.com"),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
    }

    [Fact]
    public async Task Unknown_email_returns_generic_success_without_sending()
    {
        var handler = CreateHandler(TenantId, out _, out var sender);

        var result = await handler.Handle(
            new ResendVerificationCommand("missing@b.com"),
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthResendVerificationSuccess, result.ErrorCode);
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task Already_verified_returns_generic_success_without_sending()
    {
        var handler = CreateHandler(TenantId, out var users, out var sender);
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        user.MarkEmailVerified();
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(
            new ResendVerificationCommand("a@b.com"),
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthResendVerificationSuccess, result.ErrorCode);
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task Eligible_user_gets_challenge_and_email()
    {
        var handler = CreateHandler(TenantId, out var users, out var sender);
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        Assert.False(user.IsEmailVerified);
        await users.AddAsync(user, CancellationToken.None);

        var result = await handler.Handle(
            new ResendVerificationCommand("A@B.com"),
            CancellationToken.None
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ErrorCodes.AuthResendVerificationSuccess, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(user.EmailVerificationTokenHash));
        var sent = Assert.Single(sender.Sent);
        Assert.Equal("a@b.com", sent.Email);
        Assert.Contains("/verify-email?token=", sent.Url, StringComparison.Ordinal);
    }

    private static ResendVerificationCommandHandler CreateHandler(
        Guid? tenantId,
        out InMemoryRepository<User> users,
        out FakeEmailVerificationSender sender
    )
    {
        users = new InMemoryRepository<User>();
        sender = new FakeEmailVerificationSender();
        return new ResendVerificationCommandHandler(
            users,
            sender,
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            },
            Options.Create(
                new AuthOptions
                {
                    EmailVerificationHours = 24,
                    PublicAppBaseUrl = "https://omnisync.com",
                }
            )
        );
    }

    private sealed class FakeEmailVerificationSender : IEmailVerificationSender
    {
        public List<(string Email, string Url)> Sent { get; } = [];

        public Task SendAsync(
            string email,
            string verificationUrl,
            CancellationToken cancellationToken = default
        )
        {
            Sent.Add((email, verificationUrl));
            return Task.CompletedTask;
        }
    }
}
