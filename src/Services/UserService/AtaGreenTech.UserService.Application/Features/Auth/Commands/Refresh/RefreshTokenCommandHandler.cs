using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh
{
    /// <summary>
    /// Refresh rotation + reuse detection (#27 / #70). Hatalarda yalnızca <see cref="ErrorCodes"/> taşınır;
    /// yerelleştirme API sınırında yapılır. DB iptali transaction commit’ine bağlıdır (<see cref="AuthAttemptResult"/>).
    /// </summary>
    internal sealed class RefreshTokenCommandHandler(
        IUserDbContext db,
        IRepository<RefreshToken> refreshTokens,
        IAccessTokenIssuer accessTokenIssuer,
        ICurrentTenant currentTenant,
        IOptions<AuthOptions> authOptions,
        AccessTokenLifecycle accessTokenLifecycle
    ) : ICommandHandler<RefreshTokenCommand, AuthAttemptResult>
    {
        public async Task<AuthAttemptResult> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return AuthAttemptResult.Fail(AuthErrorCodes.TenantUnresolved, statusCode: 400);

            var tokenHash = RefreshTokenFactory.Hash(request.RefreshToken);
            var existing = await refreshTokens.GetAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

            if (existing is null)
                return AuthAttemptResult.Fail(ErrorCodes.SessionExpired);

            // Reuse detection: kullanılmış/iptal token tekrar sunuldu → tüm aile iptal (transaction commit edilmeli).
            if (existing.IsRevoked)
            {
                await RevokeFamilyAsync(existing.FamilyId, cancellationToken);
                await accessTokenLifecycle.RevokeSessionAsync(existing.FamilyId, cancellationToken);
                return AuthAttemptResult.Fail(ErrorCodes.SessionReuseDetected);
            }

            if (existing.IsExpired(DateTime.UtcNow))
            {
                existing.Revoke();
                return AuthAttemptResult.Fail(ErrorCodes.SessionExpired);
            }

            var user = await db.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == existing.UserId, cancellationToken);

            if (user is null || !user.IsActive)
            {
                await RevokeFamilyAsync(existing.FamilyId, cancellationToken);
                await accessTokenLifecycle.RevokeSessionAsync(existing.FamilyId, cancellationToken);
                return AuthAttemptResult.Fail(AuthErrorCodes.AccountInactive, statusCode: 403);
            }

            // Rotation: mevcut token tek kullanımlık; aynı FamilyId ile yenisi üretilir.
            existing.Revoke();

            var roleNames = user.UserRoles
                .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
                .Select(ur => ur.Role.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var permissionNames = await UserPermissionResolver.ResolveAsync(db, user.Id, cancellationToken);

            var access = accessTokenIssuer.Create(
                user.Id,
                user.Email,
                currentTenant.Id.Value,
                existing.FamilyId,
                user.SecurityStamp,
                roleNames,
                permissionNames
            );
            var (refreshPlaintext, refreshEntity) = RefreshTokenFactory.Create(
                user.Id,
                existing.FamilyId,
                authOptions.Value.RefreshTokenDays,
                request.ClientIp,
                request.DeviceInfo
            );

            await refreshTokens.AddAsync(refreshEntity, cancellationToken);
            await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);

            return AuthAttemptResult.Ok(new AuthResponse(access.AccessToken, refreshPlaintext, access.ExpiresAt));
        }

        /// <summary>Ailedeki aktif refresh satırlarını DB’de iptal eder (access denylist ayrı).</summary>
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
}