using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword
{
    /// <summary>
    /// Uygun hesaplarda şifre sıfırlama challenge’ı üretir ve e-posta gönderir.
    /// Enumeration önlemek için uygun olmayan durumlarda da <see cref="ErrorCodes.AuthForgotPasswordSuccess"/> döner.
    /// Yerelleştirme API sınırında (#71).
    /// </summary>
    internal sealed class ForgotPasswordCommandHandler(
        IRepository<User> users,
        IPasswordResetSender passwordResetSender,
        ICurrentTenant currentTenant,
        IOptions<AuthOptions> authOptions
    ) : ICommandHandler<ForgotPasswordCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(
            ForgotPasswordCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var email = User.NormalizeEmail(request.Email);
            var user = await users.GetAsync(u => u.Email == email, cancellationToken);

            if (user is null || !user.IsActive)
                return MessageResult.OkCode(ErrorCodes.AuthForgotPasswordSuccess);

            var hours = Math.Max(1, authOptions.Value.PasswordResetHours);
            var (plaintext, hash) = EmailVerificationTokenFactory.Create();
            user.SetPasswordResetChallenge(hash, DateTime.UtcNow.AddHours(hours));

            var resetUrl = AuthPublicLinks.Build(authOptions.Value, currentTenant, "/reset-password", plaintext);
            await passwordResetSender.SendAsync(user.Email, resetUrl, cancellationToken);

            return MessageResult.OkCode(ErrorCodes.AuthForgotPasswordSuccess);
        }
    }
}