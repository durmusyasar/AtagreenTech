using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;

internal sealed class LogoutCommandHandler(
    IRepository<RefreshToken> refreshTokens,
    AccessTokenLifecycle accessTokenLifecycle
) : ICommandHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenFactory.Hash(request.RefreshToken);
        var existing = await refreshTokens.GetAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
        if (existing is null)
            return;

        var family = await refreshTokens.GetAllByFilterAsync(
            rt => rt.FamilyId == existing.FamilyId && !rt.IsRevoked,
            cancellationToken
        );
        foreach (var token in family)
            token.Revoke();

        await accessTokenLifecycle.RevokeSessionAsync(existing.FamilyId, cancellationToken);
    }
}
