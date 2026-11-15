using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    IRepository<User> users,
    IRepository<RefreshToken> refreshTokens,
    IPasswordHasher passwordHasher,
    IIdentityAuditLogger auditLogger
) : ICommandHandler<ResetPasswordCommand, MessageResult>
{
    public async Task<MessageResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = EmailVerificationTokenFactory.Hash(request.Token);
        var user = await users.GetAsync(u => u.PasswordResetTokenHash == tokenHash, cancellationToken);

        if (user is null || !user.IsPasswordResetChallengeValid(tokenHash, DateTime.UtcNow))
            return MessageResult.Fail("Geçersiz veya süresi dolmuş sıfırlama bağlantısı.");

        user.ChangePassword(passwordHasher.Hash(request.NewPassword));
        await SessionRevocation.RevokeAllRefreshTokensAsync(refreshTokens, user.Id, cancellationToken);

        await auditLogger.LogAsync(
            AuditLog.Actions.PasswordReset,
            user.Email,
            user.Id,
            request.ClientIp,
            "Şifre sıfırlama işlemi gerçekleştirildi.",
            cancellationToken
        );

        return MessageResult.Ok("Şifreniz başarıyla sıfırlandı. Tüm aktif oturumlarınız sonlandırıldı.");
    }
}
