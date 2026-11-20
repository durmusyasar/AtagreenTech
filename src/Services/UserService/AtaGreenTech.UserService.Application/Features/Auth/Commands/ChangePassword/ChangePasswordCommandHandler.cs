using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword
{
    /// <summary>
    /// Mevcut parolayı doğrular, yenisini yazar, tüm oturumları iptal eder ve stamp yayınlar.
    /// Hata/başarı yalnızca <see cref="ErrorCodes"/> taşır; yerelleştirme API’de (#71).
    /// Yanlış mevcut parola 401 değildir (istemci oturum düşürmesin).
    /// </summary>
    internal sealed class ChangePasswordCommandHandler(
        IRepository<User> users,
        IRepository<RefreshToken> refreshTokens,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        AccessTokenLifecycle accessTokenLifecycle
    ) : ICommandHandler<ChangePasswordCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.IsAuthenticated
                || string.IsNullOrWhiteSpace(currentUser.UserId)
                || !Guid.TryParse(currentUser.UserId, out var userId))
                return MessageResult.Fail(ErrorCodes.AuthInvalidCredentials);

            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var user = await users.GetByIdAsync(userId, cancellationToken);
            if (user is null || !user.IsActive)
                return MessageResult.Fail(ErrorCodes.AuthInvalidCredentials);

            // Yanlış şifre 401 olmamalı: istemci 401'i oturum düşmesi sanıp refresh/signOut yapıyor.
            if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                return MessageResult.Fail(ErrorCodes.AuthPasswordMismatch);

            user.ChangePassword(passwordHasher.Hash(request.NewPassword));
            await SessionRevocation.RevokeAllRefreshTokensAsync(
                refreshTokens,
                user.Id,
                cancellationToken,
                accessTokenLifecycle
            );
            await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);

            return MessageResult.OkCode(ErrorCodes.AuthChangePasswordSuccess);
        }
    }
}