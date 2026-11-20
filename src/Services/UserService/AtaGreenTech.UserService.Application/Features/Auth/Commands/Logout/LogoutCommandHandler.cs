using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout
{
    /// <summary>
    /// Refresh ailesini DB’de iptal eder ve access <c>sid</c> denylist kaydı yazar (#70).
    /// Tenant çözülemezse <see cref="ErrorCodes.AuthTenantUnresolved"/>; aksi halde
    /// <see cref="ErrorCodes.AuthLogoutSuccess"/> döner (yerelleştirme API’de). Token yoksa
    /// veya zaten iptalse idempotent başarı (bilgi sızdırmaz).
    /// </summary>
    internal sealed class LogoutCommandHandler(
        IRepository<RefreshToken> refreshTokens,
        AccessTokenLifecycle accessTokenLifecycle,
        ICurrentTenant currentTenant
    ) : ICommandHandler<LogoutCommand, MessageResult>
    {
        public async Task<MessageResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return MessageResult.Fail(ErrorCodes.AuthTenantUnresolved);

            var tokenHash = RefreshTokenFactory.Hash(request.RefreshToken);
            var existing = await refreshTokens.GetAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
            if (existing is null)
                return MessageResult.OkCode(ErrorCodes.AuthLogoutSuccess);

            await RevokeFamilyAsync(existing.FamilyId, cancellationToken);
            await accessTokenLifecycle.RevokeSessionAsync(existing.FamilyId, cancellationToken);

            return MessageResult.OkCode(ErrorCodes.AuthLogoutSuccess);
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