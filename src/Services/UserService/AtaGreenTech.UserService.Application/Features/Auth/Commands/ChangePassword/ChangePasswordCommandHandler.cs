using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IRepository<User> users,
    IRepository<RefreshToken> refreshTokens,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    AccessTokenLifecycle accessTokenLifecycle
) : ICommandHandler<ChangePasswordCommand, MessageResult>
{
    public async Task<MessageResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            throw new UnauthorizedAccessException("Geçerli bir oturum bulunamadı.");

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

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

        return MessageResult.Ok("Şifreniz başarıyla değiştirildi. Lütfen tekrar giriş yapın.");
    }
}
