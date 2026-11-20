using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa;

internal sealed class VerifyMfaCommandHandler(
    IRepository<User> users,
    ITotpService totpService,
    IIdentityAuditLogger auditLogger,
    ICurrentUser currentUser
) : ICommandHandler<VerifyMfaCommand, MessageResult>
{
    public async Task<MessageResult> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            throw new UnauthorizedAccessException("Geçerli bir oturum bulunamadı.");

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

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
            details: "Kullanıcı TOTP MFA'yı etkinleştirdi.",
            cancellationToken: cancellationToken
        );

        return MessageResult.Ok("MFA başarıyla etkinleştirildi.");
    }
}
