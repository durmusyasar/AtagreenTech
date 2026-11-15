using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword;

internal sealed class ForgotPasswordCommandHandler(
    IRepository<User> users,
    IPasswordResetSender passwordResetSender,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
) : ICommandHandler<ForgotPasswordCommand, MessageResult>
{
    private const string GenericSuccess =
        "Eğer e-posta sistemimizde kayıtlıysa sıfırlama bağlantısı gönderilmiştir.";

    public async Task<MessageResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email);
        var user = await users.GetAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
            return MessageResult.Ok(GenericSuccess);

        var hours = Math.Max(1, authOptions.Value.PasswordResetHours);
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        user.SetPasswordResetChallenge(hash, DateTime.UtcNow.AddHours(hours));

        var resetUrl = AuthPublicLinks.Build(
            authOptions.Value,
            currentTenant,
            "/reset-password",
            plaintext
        );
        await passwordResetSender.SendAsync(user.Email, resetUrl, cancellationToken);

        return MessageResult.Ok(GenericSuccess);
    }
}
