using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa
{
    /// <summary>
    /// Parola doğrulayıp MFA’yı kapatır. Hata/başarı ve audit <c>details</c> <see cref="ErrorCodes"/> +
    /// <see cref="IErrorLocalizer"/> (#71). Yanlış parola 401 değildir.
    /// </summary>
    internal sealed class DisableMfaCommandHandler(
        IRepository<User> users,
        IPasswordHasher passwordHasher,
        IIdentityAuditLogger auditLogger,
        ICurrentUser currentUser,
        IErrorLocalizer localizer
    ) : ICommandHandler<DisableMfaCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
        {
            if (!currentUser.IsAuthenticated
                || string.IsNullOrWhiteSpace(currentUser.UserId)
                || !Guid.TryParse(currentUser.UserId, out var userId))
                return MessageResult.Fail(ErrorCodes.AuthInvalidCredentials);

            var user = await users.GetByIdAsync(userId, cancellationToken);
            if (user is null || !user.IsActive)
                return MessageResult.Fail(ErrorCodes.AuthInvalidCredentials);

            // Yanlış şifre 401 olmamalı: istemci 401'i oturum düşmesi sanıp refresh/signOut yapıyor.
            if (!passwordHasher.Verify(request.Password, user.PasswordHash))
                return MessageResult.Fail(ErrorCodes.AuthPasswordMismatch);

            if (!user.IsMfaEnabled && string.IsNullOrWhiteSpace(user.TotpSecret))
                return MessageResult.Fail(ErrorCodes.AuthMfaAlreadyDisabled);

            user.DisableMfa();
            await auditLogger.LogAsync(
                AuditLog.Actions.MfaDisabled,
                user.Email,
                user.Id,
                details: localizer.Localize(ErrorCodes.Audit.MfaDisabled),
                cancellationToken: cancellationToken);

            return MessageResult.OkCode(ErrorCodes.AuthDisableMfaSuccess);
        }
    }
}