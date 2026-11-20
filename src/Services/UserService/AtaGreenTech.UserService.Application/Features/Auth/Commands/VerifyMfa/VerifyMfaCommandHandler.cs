using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa
{
    /// <summary>
    /// Enrollment secret ile TOTP doğrular; başarılıysa MFA’yı açar.
    /// Hata/başarı ve audit <c>details</c> yalnızca <see cref="ErrorCodes"/> taşır; metin <see cref="IErrorLocalizer"/> (#71).
    /// </summary>
    internal sealed class VerifyMfaCommandHandler(
        IRepository<User> users,
        ITotpService totpService,
        IIdentityAuditLogger auditLogger,
        ICurrentUser currentUser,
        IErrorLocalizer localizer
    ) : ICommandHandler<VerifyMfaCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
        {
            if (!currentUser.IsAuthenticated
                || string.IsNullOrWhiteSpace(currentUser.UserId)
                || !Guid.TryParse(currentUser.UserId, out var userId))
                return MessageResult.Fail(ErrorCodes.AuthInvalidCredentials);

            var user = await users.GetByIdAsync(userId, cancellationToken);
            if (user is null || !user.IsActive)
                return MessageResult.Fail(ErrorCodes.AuthInvalidCredentials);

            if (user.IsMfaEnabled)
                return MessageResult.Fail(ErrorCodes.AuthMfaAlreadyEnabled);

            if (string.IsNullOrWhiteSpace(user.TotpSecret))
                return MessageResult.Fail(ErrorCodes.AuthMfaSetupRequired);

            if (!totpService.VerifyCode(user.TotpSecret, request.Code))
                return MessageResult.Fail(ErrorCodes.AuthMfaInvalid);

            user.EnableMfa();
            await auditLogger.LogAsync(
                AuditLog.Actions.MfaEnabled,
                user.Email,
                user.Id,
                details: localizer.Localize(ErrorCodes.Audit.MfaEnabled),
                cancellationToken: cancellationToken);

            return MessageResult.OkCode(ErrorCodes.AuthVerifyMfaSuccess);
        }
    }
}