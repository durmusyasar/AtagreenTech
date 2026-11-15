using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Refresh token oturumu. Düz metin token yalnızca istemciye verilir; veritabanında yalnızca
/// <see cref="TokenHash"/> saklanır. <see cref="FamilyId"/> rotation ve reuse detection için aile kimliğidir.
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
            throw new ArgumentException("User id is required.", nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (familyId == Guid.Empty)
            throw new ArgumentException("Family id is required.", nameof(familyId));
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiry must be in the future.", nameof(expiresAt));

        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        ExpiresAt = expiresAt;
        CreatedByIp = string.IsNullOrWhiteSpace(createdByIp) ? null : createdByIp.Trim();
        DeviceInfo = NormalizeDeviceInfo(deviceInfo);
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

    private static string? NormalizeDeviceInfo(string? deviceInfo)
    {
        if (string.IsNullOrWhiteSpace(deviceInfo))
            return null;

        var trimmed = deviceInfo.Trim();
        return trimmed.Length <= DeviceInfoMaxLength
            ? trimmed
            : trimmed[..DeviceInfoMaxLength];
    }
}

