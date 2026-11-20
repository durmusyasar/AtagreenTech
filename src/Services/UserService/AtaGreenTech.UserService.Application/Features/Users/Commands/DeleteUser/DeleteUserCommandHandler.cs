using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler(
    IRepository<User> users,
    IRepository<RefreshToken> refreshTokens,
    IUserDbContext db,
    ICurrentUser currentUser,
    AccessTokenLifecycle accessTokenLifecycle
) : ICommandHandler<DeleteUserCommand, MessageResult>
{
    public async Task<MessageResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var adminId))
            throw new UnauthorizedAccessException("Geçerli bir oturum bulunamadı.");

        if (request.Id == adminId)
            return MessageResult.Fail("Kendi hesabınızı silemezsiniz.");

        var user = await users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        var isAdmin = await db.UserRoles.AnyAsync(
            ur => ur.UserId == user.Id && ur.Role.Name == Role.Names.Admin,
            cancellationToken
        );
        if (isAdmin)
        {
            var otherActiveAdmins = await db.UserRoles.CountAsync(
                ur => ur.UserId != user.Id
                    && ur.Role.Name == Role.Names.Admin
                    && ur.User.IsActive,
                cancellationToken
            );
            if (otherActiveAdmins == 0)
                return MessageResult.Fail("Son aktif Admin hesabı silinemez.");
        }

        user.Deactivate();
        user.InvalidateCredentialStamp();
        await SessionRevocation.RevokeAllRefreshTokensAsync(
            refreshTokens,
            user.Id,
            cancellationToken,
            accessTokenLifecycle
        );
        await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);
        users.Delete(user);

        return MessageResult.Ok("Kullanıcı silindi.");
    }
}
