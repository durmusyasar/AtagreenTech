using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Queries.GetSessions;

internal sealed class GetSessionsQueryHandler(IUserDbContext db, ICurrentUser currentUser)
    : IQueryHandler<GetSessionsQuery, IReadOnlyList<SessionDto>>
{
    public async Task<IReadOnlyList<SessionDto>> Handle(
        GetSessionsQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId(currentUser);
        var now = DateTime.UtcNow;
        string? currentHash = string.IsNullOrWhiteSpace(request.CurrentRefreshToken)
            ? null
            : RefreshTokenFactory.Hash(request.CurrentRefreshToken.Trim());

        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > now)
            .OrderByDescending(rt => rt.CreatedAt)
            .ThenBy(rt => rt.Id)
            .ToListAsync(cancellationToken);

        return tokens
            .Select(t => new SessionDto(
                t.Id,
                t.CreatedByIp,
                t.DeviceInfo,
                t.CreatedAt,
                t.ExpiresAt,
                currentHash is not null
                    && string.Equals(t.TokenHash, currentHash, StringComparison.Ordinal)
            ))
            .ToArray();
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
