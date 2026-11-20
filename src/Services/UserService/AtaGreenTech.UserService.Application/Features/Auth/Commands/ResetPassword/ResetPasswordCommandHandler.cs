using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword
{
    /// <summary>
    /// Reset jetonunu doğrular, parolayı değiştirir, refresh/access oturumlarını iptal eder ve stamp yayınlar.
    /// Hata/başarı ve audit <c>details</c> <see cref="ErrorCodes"/> + <see cref="IErrorLocalizer"/> (#71).
    /// </summary>
    internal sealed class ResetPasswordCommandHandler(
        IRepository<User> users,
        IRepository<RefreshToken> refreshTokens,
        IPasswordHasher passwordHasher,
        IIdentityAuditLogger auditLogger,
        AccessTokenLifecycle accessTokenLifecycle,
        ICurrentTenant currentTenant,
        IErrorLocalizer localizer
    ) : ICommandHandler<ResetPasswordCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var tokenHash = EmailVerificationTokenFactory.Hash(request.Token);
            var user = await users.GetAsync(u => u.PasswordResetTokenHash == tokenHash, cancellationToken);

            if (user is null || !user.IsPasswordResetChallengeValid(tokenHash, DateTime.UtcNow))
                return MessageResult.Fail(ErrorCodes.AuthResetTokenInvalid);

            user.ChangePassword(passwordHasher.Hash(request.NewPassword));
            await SessionRevocation.RevokeAllRefreshTokensAsync(
                refreshTokens,
                user.Id,
                cancellationToken,
                accessTokenLifecycle
            );
            await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);

            await auditLogger.LogAsync(
                AuditLog.Actions.PasswordReset,
                user.Email,
                user.Id,
                request.ClientIp,
                details: localizer.Localize(ErrorCodes.Audit.PasswordReset),
                cancellationToken
            );

            return MessageResult.OkCode(ErrorCodes.AuthResetPasswordSuccess);
        }
    }
}