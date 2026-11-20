using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite
{
    /// <summary>
    /// Davet jetonunu hash’leyip challenge ile eşleştirir; kabulde parola/profil yazar.
    /// Hata/başarı yalnızca <see cref="ErrorCodes"/> taşır; yerelleştirme API’de (#71).
    /// </summary>
    internal sealed class AcceptInviteCommandHandler(
        IRepository<User> users,
        IPasswordHasher passwordHasher,
        ICurrentTenant currentTenant) : ICommandHandler<AcceptInviteCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(AcceptInviteCommand request, CancellationToken cancellationToken)
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var tokenHash = EmailVerificationTokenFactory.Hash(request.Token);
            var user = await users.GetAsync(u => u.InvitationTokenHash == tokenHash, cancellationToken);

            if (user is null || !user.IsInvitationChallengeValid(tokenHash, DateTime.UtcNow))
                return MessageResult.Fail(ErrorCodes.AuthInviteTokenInvalid);

            user.AcceptInvitation(passwordHasher.Hash(request.Password), request.FirstName, request.LastName);

            return MessageResult.OkCode(ErrorCodes.AuthAcceptInviteSuccess);
        }
    }
}