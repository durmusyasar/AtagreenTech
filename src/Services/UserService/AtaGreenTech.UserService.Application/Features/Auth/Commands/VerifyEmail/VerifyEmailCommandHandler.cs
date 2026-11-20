using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail
{
    /// <summary>
    /// Doğrulama jetonunu hash’leyip tenant DB’de challenge ile eşleştirir; başarılıysa e-postayı doğrulanmış işaretler.
    /// Hata/başarı yalnızca <see cref="ErrorCodes"/> taşır; yerelleştirme API’de (#71).
    /// </summary>
    internal sealed class VerifyEmailCommandHandler(
        IRepository<User> users,
        ICurrentTenant currentTenant
    ) : ICommandHandler<VerifyEmailCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var tokenHash = EmailVerificationTokenFactory.Hash(request.Token);
            var user = await users.GetAsync(u => u.EmailVerificationTokenHash == tokenHash, cancellationToken);

            if (user is null || !user.IsEmailVerificationChallengeValid(tokenHash, DateTime.UtcNow))
                return MessageResult.Fail(ErrorCodes.AuthVerifyTokenInvalid);

            user.MarkEmailVerified();
            return MessageResult.OkCode(ErrorCodes.AuthVerifyEmailSuccess);
        }
    }
}