using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;

internal sealed class ResendVerificationCommandHandler(
    IRepository<User> users,
    IEmailVerificationSender emailSender,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
) : ICommandHandler<ResendVerificationCommand, MessageResult>
{
    private const string GenericSuccess = "Doğrulama e-postası gönderildi (hesap uygunsa).";

    public async Task<MessageResult> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email);
        var user = await users.GetAsync(u => u.Email == email, cancellationToken);

        if (user is null || user.IsEmailVerified || !user.IsActive)
            return MessageResult.Ok(GenericSuccess);

        var hours = Math.Max(1, authOptions.Value.EmailVerificationHours);
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        user.SetEmailVerificationChallenge(hash, DateTime.UtcNow.AddHours(hours));

        var verifyUrl = BuildVerificationUrl(plaintext);
        await emailSender.SendAsync(user.Email, verifyUrl, cancellationToken);

        return MessageResult.Ok(GenericSuccess);
    }

    private string BuildVerificationUrl(string plaintextToken)
    {
        var baseUrl = authOptions.Value.PublicAppBaseUrl.TrimEnd('/');
        var tenant = currentTenant.Identifier;
        // Tenant subdomain varsa {tenant}.host; yoksa düz base URL.
        if (!string.IsNullOrWhiteSpace(tenant)
            && Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            && uri.Host.Split('.').Length >= 2)
        {
            var builder = new UriBuilder(uri) { Host = $"{tenant}.{uri.Host}" };
            baseUrl = builder.Uri.GetLeftPart(UriPartial.Authority);
        }

        return $"{baseUrl}/verify-email?token={Uri.EscapeDataString(plaintextToken)}";
    }
}
