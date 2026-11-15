using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeSession;

internal sealed class RevokeSessionCommandHandler(
    IRepository<RefreshToken> refreshTokens,
    ICurrentUser currentUser
) : ICommandHandler<RevokeSessionCommand, MessageResult>
{
    public async Task<MessageResult> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId(currentUser);

        var session = await refreshTokens.GetAsync(
            rt => rt.Id == request.SessionId && rt.UserId == userId,
            cancellationToken
        ) ?? throw new NotFoundException(nameof(RefreshToken), request.SessionId);

        session.Revoke();
        return MessageResult.Ok("Oturum sonlandırıldı.");
    }

    private static Guid RequireUserId(ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            throw new UnauthorizedAccessException("Geçerli bir oturum bulunamadı.");

        return userId;
    }
}
