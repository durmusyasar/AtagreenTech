using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Access stamp yayınlama ve oturum denylist işlemleri (#70).
    /// Login/refresh sonrası <c>sst</c> yayını; logout / session revoke / şifre değişiminde <c>sid</c> iptali.
    /// </summary>
    public sealed class AccessTokenLifecycle(
        IUserAccessStampStore stamps,
        IAccessTokenRevocationStore revocations,
        ICurrentTenant currentTenant,
        IOptions<AuthOptions> authOptions
    )
    {
        /// <summary>
        /// Access token denylist TTL’si; <c>Auth:AccessTokenMinutes</c> (≤0 → <see cref="JwtContract.DefaultAccessTokenMinutes"/>).
        /// </summary>
        public TimeSpan AccessTokenTtl
        {
            get
            {
                var minutes = authOptions.Value.AccessTokenMinutes;
                if (minutes <= 0)
                    minutes = JwtContract.DefaultAccessTokenMinutes;
                return TimeSpan.FromMinutes(minutes);
            }
        }

        /// <summary>
        /// Kullanıcı <c>SecurityStamp</c> değerini mevcut tenant kapsamında yayınlar (Stock sst doğrulaması).
        /// </summary>
        public async Task PublishStampAsync(
            Guid userId,
            string securityStamp,
            CancellationToken cancellationToken
        )
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(userId));
            if (string.IsNullOrWhiteSpace(securityStamp))
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(securityStamp));

            if (currentTenant.Id is not { } tenantId)
                throw new InvalidOperationException(ErrorCodes.AuthTenantUnresolved);

            await stamps.PublishAsync(tenantId, userId, securityStamp.Trim(), cancellationToken);
        }

        /// <summary>
        /// Refresh ailesi / access <c>sid</c> için denylist kaydı (TTL = access ömrü).
        /// </summary>
        public Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            if (sessionId == Guid.Empty)
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(sessionId));

            return revocations.RevokeSessionAsync(sessionId, AccessTokenTtl, cancellationToken);
        }

        /// <summary>Birden fazla oturum ailesini tekilleştirerek iptal eder.</summary>
        public async Task RevokeFamiliesAsync(
            IEnumerable<Guid> familyIds,
            CancellationToken cancellationToken
        )
        {
            foreach (var familyId in familyIds.Where(id => id != Guid.Empty).Distinct())
                await RevokeSessionAsync(familyId, cancellationToken);
        }

        /// <summary>
        /// Kullanıcının tüm aktif refresh oturumlarını DB’de iptal eder ve ilgili <c>sid</c> ailelerini denylist’e yazar.
        /// </summary>
        public async Task RevokeAllSessionsAsync(
            IRepository<RefreshToken> refreshTokens,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(userId));

            var active = await refreshTokens.GetAllByFilterAsync(
                rt => rt.UserId == userId && !rt.IsRevoked,
                cancellationToken
            );

            var families = new HashSet<Guid>();
            foreach (var token in active)
            {
                families.Add(token.FamilyId);
                token.Revoke();
            }

            await RevokeFamiliesAsync(families, cancellationToken);
        }
    }
}