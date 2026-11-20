using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;

internal sealed class AcceptInviteCommandHandler(
    IRepository<User> users,
    IPasswordHasher passwordHasher
) : ICommandHandler<AcceptInviteCommand, MessageResult>
{
    public async Task<MessageResult> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = EmailVerificationTokenFactory.Hash(request.Token);
        var user = await users.GetAsync(u => u.InvitationTokenHash == tokenHash, cancellationToken);

        if (user is null || !user.IsInvitationChallengeValid(tokenHash, DateTime.UtcNow))
            return MessageResult.Fail(ErrorCodes.AuthInviteTokenInvalid);

        user.AcceptInvitation(passwordHasher.Hash(request.Password), request.FirstName, request.LastName);
        return MessageResult.Ok("Hesabınız başarıyla oluşturuldu. Artık giriş yapabilirsiniz.");
    }
}
