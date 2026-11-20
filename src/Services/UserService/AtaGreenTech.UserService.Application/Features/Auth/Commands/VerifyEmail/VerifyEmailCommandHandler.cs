using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;

internal sealed class VerifyEmailCommandHandler(IRepository<User> users)
    : ICommandHandler<VerifyEmailCommand, MessageResult>
{
    public async Task<MessageResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = EmailVerificationTokenFactory.Hash(request.Token);
        var user = await users.GetAsync(u => u.EmailVerificationTokenHash == tokenHash, cancellationToken);

        if (user is null || !user.IsEmailVerificationChallengeValid(tokenHash, DateTime.UtcNow))
            return MessageResult.Fail(ErrorCodes.AuthVerifyTokenInvalid);

        user.MarkEmailVerified();
        return MessageResult.Ok("E-posta adresiniz başarıyla doğrulandı.");
    }
}
