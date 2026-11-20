using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification
{
    /// <summary>
    /// Uygun hesaplarda yeni doğrulama challenge’ı üretir ve e-posta gönderir.
    /// Enumeration önlemek için uygun olmayan durumlarda da <see cref="ErrorCodes.AuthResendVerificationSuccess"/> döner.
    /// Yerelleştirme API sınırında (#71).
    /// </summary>
    internal sealed class ResendVerificationCommandHandler(
        IRepository<User> users,
        IEmailVerificationSender emailSender,
        ICurrentTenant currentTenant,
        IOptions<AuthOptions> authOptions) : ICommandHandler<ResendVerificationCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var email = User.NormalizeEmail(request.Email);
            var user = await users.GetAsync(u => u.Email == email, cancellationToken);

            if (user is null || user.IsEmailVerified || !user.IsActive)
                return MessageResult.OkCode(ErrorCodes.AuthResendVerificationSuccess);

            var hours = Math.Max(1, authOptions.Value.EmailVerificationHours);
            var (plaintext, hash) = EmailVerificationTokenFactory.Create();
            user.SetEmailVerificationChallenge(hash, DateTime.UtcNow.AddHours(hours));

            var verifyUrl = AuthPublicLinks.Build(
                authOptions.Value,
                currentTenant,
                "/verify-email",
                plaintext
            );
            await emailSender.SendAsync(user.Email, verifyUrl, cancellationToken);

            return MessageResult.OkCode(ErrorCodes.AuthResendVerificationSuccess);
        }
    }
}