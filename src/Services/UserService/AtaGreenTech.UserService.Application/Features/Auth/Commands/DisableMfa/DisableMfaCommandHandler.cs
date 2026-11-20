using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa;

internal sealed class DisableMfaCommandHandler(
    IRepository<User> users,
    IPasswordHasher passwordHasher,
    IIdentityAuditLogger auditLogger,
    ICurrentUser currentUser
) : ICommandHandler<DisableMfaCommand, MessageResult>
{
    public async Task<MessageResult> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            throw new UnauthorizedAccessException("Geçerli bir oturum bulunamadı.");

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

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
            details: "Kullanıcı MFA'yı kapattı.",
            cancellationToken: cancellationToken
        );

        return MessageResult.Ok("MFA devre dışı bırakıldı.");
    }
}
