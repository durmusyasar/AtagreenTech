using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Refresh token oturumu. Düz metin token yalnızca istemciye verilir; veritabanında yalnızca
    /// <see cref="TokenHash"/> saklanır. <see cref="FamilyId"/> rotation ve reuse detection için aile kimliğidir.
    /// Invariant ihlallerinde <see cref="DomainErrorCodes"/> taşınır (#71).
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        public const int TokenHashMaxLength = 128;
        public const int CreatedByIpMaxLength = 64;
        public const int DeviceInfoMaxLength = 512;

        protected RefreshToken()
        {
            TokenHash = null!;
            User = null!;
        }

        public RefreshToken(
            Guid userId,
            string tokenHash,
            Guid familyId,
            DateTime expiresAt,
            string? createdByIp = null,
            string? deviceInfo = null
        )
            : this()
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(DomainErrorCodes.ValidationRequired, nameof(userId));
            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new ArgumentException(DomainErrorCodes.ValidationRequired, nameof(tokenHash));
            if (tokenHash.Length > TokenHashMaxLength)
                throw new ArgumentException(DomainErrorCodes.ValidationMaxLength, nameof(tokenHash));
            if (familyId == Guid.Empty)
                throw new ArgumentException(DomainErrorCodes.ValidationRequired, nameof(familyId));
            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException(DomainErrorCodes.ValidationInvalid, nameof(expiresAt));

            UserId = userId;
            TokenHash = tokenHash;
            FamilyId = familyId;
            ExpiresAt = expiresAt;
            CreatedByIp = NormalizeBounded(createdByIp, CreatedByIpMaxLength);
            DeviceInfo = NormalizeBounded(deviceInfo, DeviceInfoMaxLength);
        }

        public Guid UserId { get; private set; }

        public User User { get; private set; }

        /// <summary>Refresh token'ın SHA-256 (veya eşdeğer) özeti; düz metin saklanmaz.</summary>
        public string TokenHash { get; private set; }

        /// <summary>Aynı login zincirindeki token ailesi; reuse tespitinde tüm aile iptal edilir.</summary>
        public Guid FamilyId { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public bool IsRevoked { get; private set; }

        public DateTime? RevokedAt { get; private set; }

        /// <summary>Oturumu oluşturan istemci IP'si (taslaktaki IpAddress karşılığı).</summary>
        public string? CreatedByIp { get; private set; }

        /// <summary>User-Agent veya cihaz özeti; oturum listesinde gösterilir.</summary>
        public string? DeviceInfo { get; private set; }

        public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

        public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

        public void Revoke(DateTime? revokedAtUtc = null)
        {
            if (IsRevoked)
                return;

            IsRevoked = true;
            RevokedAt = revokedAtUtc ?? DateTime.UtcNow;
        }

        /// <summary>Boşsa null; aksi halde trim + üst sınırda kesme (telemetri alanı).</summary>
        private static string? NormalizeBounded(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }
    }
}