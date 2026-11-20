using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Kullanıcının tüm aktif refresh oturumlarını iptal eder
    /// (şifre değişimi / sıfırlama / hesap kapatma).
    /// </summary>
    internal static class SessionRevocation
    {
        /// <summary>
        /// Aktif refresh satırlarını <see cref="RefreshToken.Revoke"/> ile iptal eder.
        /// <paramref name="lifecycle"/> verilirse access <c>sid</c> denylist de yazılır (#70);
        /// verilmezse yalnızca DB refresh iptali yapılır.
        /// </summary>
        /// <param name="refreshTokens">Tenant refresh token deposu.</param>
        /// <param name="userId">Oturumları iptal edilecek kullanıcı; <see cref="Guid.Empty"/> olamaz.</param>
        /// <param name="cancellationToken">İptal jetonu.</param>
        /// <param name="lifecycle">
        /// Verilirse <see cref="AccessTokenLifecycle.RevokeAllSessionsAsync"/> çağrılır
        /// (refresh + denylist). <see langword="null"/> ise yalnızca refresh satırları iptal edilir.
        /// </param>
        /// <returns>İptal işlemini tamamlayan görev.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="refreshTokens"/> null ise.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="userId"/> boşsa; ileti <see cref="ErrorCodes.ValidationRequired"/> (#71).
        /// </exception>
        public static Task RevokeAllRefreshTokensAsync(
            IRepository<RefreshToken> refreshTokens,
            Guid userId,
            CancellationToken cancellationToken,
            AccessTokenLifecycle? lifecycle = null
        )
        {
            ArgumentNullException.ThrowIfNull(refreshTokens);
            if (userId == Guid.Empty)
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(userId));

            return lifecycle is null
                ? RevokeRefreshOnlyAsync(refreshTokens, userId, cancellationToken)
                : lifecycle.RevokeAllSessionsAsync(refreshTokens, userId, cancellationToken);
        }

        /// <summary>Yalnızca DB’deki aktif refresh satırlarını iptal eder (denylist yok).</summary>
        private static async Task RevokeRefreshOnlyAsync(
            IRepository<RefreshToken> refreshTokens,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var active = await refreshTokens.GetAllByFilterAsync(
                rt => rt.UserId == userId && !rt.IsRevoked,
                cancellationToken
            );

            foreach (var token in active)
                token.Revoke();
        }
    }
}