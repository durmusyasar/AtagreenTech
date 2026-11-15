using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh;

internal sealed class RefreshTokenCommandHandler(
    IUserDbContext db,
    IRepository<RefreshToken> refreshTokens,
    IAccessTokenIssuer accessTokenIssuer,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
) : ICommandHandler<RefreshTokenCommand, AuthAttemptResult>
{
    public async Task<AuthAttemptResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            throw new InvalidOperationException("Tenant could not be resolved from the host.");

        var tokenHash = RefreshTokenFactory.Hash(request.RefreshToken);
        var existing = await refreshTokens.GetAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existing is null)
            return AuthAttemptResult.Fail("Geçersiz oturum belirteci.");

        // Reuse detection: kullanılmış/iptal token tekrar sunuldu → tüm aile iptal (transaction commit edilmeli).
        if (existing.IsRevoked)
        {
            await RevokeFamilyAsync(existing.FamilyId, cancellationToken);
            return AuthAttemptResult.Fail("Güvenlik ihlali tespit edildi. Lütfen tekrar giriş yapın.");
        }

        if (existing.IsExpired(DateTime.UtcNow))
        {
            existing.Revoke();
            return AuthAttemptResult.Fail("Oturum süresi dolmuş. Lütfen tekrar giriş yapın.");
        }

        var user = await db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            await RevokeFamilyAsync(existing.FamilyId, cancellationToken);
            return AuthAttemptResult.Fail("Hesabınız devre dışı bırakılmıştır.");
        }

        // Rotation: mevcut token tek kullanımlık; aynı FamilyId ile yenisi üretilir.
        existing.Revoke();

        var roleNames = user.UserRoles
            .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
            .Select(ur => ur.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var access = accessTokenIssuer.Create(user.Id, user.Email, currentTenant.Id.Value, roleNames);
        var (refreshPlaintext, refreshEntity) = RefreshTokenFactory.Create(
            user.Id,
            existing.FamilyId,
            authOptions.Value.RefreshTokenDays,
            request.ClientIp
        );

        await refreshTokens.AddAsync(refreshEntity, cancellationToken);

        return AuthAttemptResult.Ok(new AuthResponse(access.AccessToken, refreshPlaintext, access.ExpiresAt));
    }

    private async Task RevokeFamilyAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var family = await refreshTokens.GetAllByFilterAsync(
            rt => rt.FamilyId == familyId && !rt.IsRevoked,
            cancellationToken
        );

        foreach (var token in family)
            token.Revoke();
    }
}
