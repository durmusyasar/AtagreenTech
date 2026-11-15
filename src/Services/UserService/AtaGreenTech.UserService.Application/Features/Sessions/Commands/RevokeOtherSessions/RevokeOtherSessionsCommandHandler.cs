using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeOtherSessions;

internal sealed class RevokeOtherSessionsCommandHandler(
    IRepository<RefreshToken> refreshTokens,
    ICurrentUser currentUser
) : ICommandHandler<RevokeOtherSessionsCommand, MessageResult>
{
    public async Task<MessageResult> Handle(
        RevokeOtherSessionsCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId(currentUser);
        var currentHash = RefreshTokenFactory.Hash(request.CurrentRefreshToken.Trim());

        var others = await refreshTokens.GetAllByFilterAsync(
            rt => rt.UserId == userId && !rt.IsRevoked && rt.TokenHash != currentHash,
            cancellationToken
        );

        foreach (var session in others)
            session.Revoke();

        return MessageResult.Ok("Diğer tüm aktif oturumlarınız sonlandırıldı.");
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
