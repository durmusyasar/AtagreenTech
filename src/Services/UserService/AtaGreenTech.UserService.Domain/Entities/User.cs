using System.Text.Json;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Tenant veritabanındaki kimlik kullanıcısı. Tenant izolasyonu veritabanı düzeyinde olduğu için
/// <c>TenantId</c> kolonu tutulmaz; JWT'deki <c>tenant_id</c> claim'i host çözümlemesinden gelir.
/// </summary>
public class User : BaseEntity
{
    public const int EmailMaxLength = 256;
    public const int UsernameMaxLength = 100;
    public const int PasswordHashMaxLength = 512;
    public const int NameMaxLength = 100;
    public const int SecurityStampMaxLength = 64;
    public const int EmailVerificationTokenHashMaxLength = 128;
    public const int PasswordResetTokenHashMaxLength = 128;
    public const int InvitationTokenHashMaxLength = 128;
    public const int TotpSecretMaxLength = 64;
    public const int RecoveryCodeHashMaxLength = 128;

    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];
    private List<string> _recoveryCodeHashes = [];

    protected User()
    {
        Email = null!;
        Username = null!;
        PasswordHash = null!;
        FirstName = null!;
        LastName = null!;
        SecurityStamp = null!;
    }

    public User(
        string email,
        string username,
        string passwordHash,
        string firstName,
        string lastName
    )
        : this()
    {
        SetIdentity(email, username);
        SetProfile(firstName, lastName);
        SetPasswordHash(passwordHash);
        RotateSecurityStamp();
    }

    /// <summary>Normalize edilmiş (küçük harf) e-posta; silinmemiş kullanıcılar arasında benzersizdir.</summary>
    public string Email { get; private set; }

    /// <summary>Giriş adı; silinmemiş kullanıcılar arasında benzersizdir.</summary>
    public string Username { get; private set; }

    /// <summary>PBKDF2 salt.hash biçiminde saklanan parola özeti; düz metin asla tutulmaz.</summary>
    public string PasswordHash { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsEmailVerified { get; private set; }

    /// <summary>E-posta doğrulama token'ının SHA-256 özeti; düz metin saklanmaz.</summary>
    public string? EmailVerificationTokenHash { get; private set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    /// <summary>Şifre sıfırlama token'ının SHA-256 özeti; düz metin saklanmaz.</summary>
    public string? PasswordResetTokenHash { get; private set; }

    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    /// <summary>Davet token'ının SHA-256 özeti; düz metin saklanmaz.</summary>
    public string? InvitationTokenHash { get; private set; }

    public DateTime? InvitationTokenExpiresAt { get; private set; }

    /// <summary>Ardışık başarısız giriş denemesi sayısı; başarılı girişte sıfırlanır.</summary>
    public int AccessFailedCount { get; private set; }

    /// <summary>UTC kilit bitiş zamanı; dolana kadar giriş reddedilir.</summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>TOTP MFA etkin mi. Kurulum doğrulanana kadar <see langword="false"/> kalır.</summary>
    public bool IsMfaEnabled { get; private set; }

    /// <summary>Authenticator Base32 gizli anahtarı; MFA kapalıyken null olabilir (kurulum beklerken dolu).</summary>
    public string? TotpSecret { get; private set; }

    /// <summary>Kurtarma kodu SHA-256 özetlerinin JSON dizisi; düz metin saklanmaz.</summary>
    public string? RecoveryCodeHashesJson { get; private set; }

    /// <summary>
    /// Parola veya kritik kimlik değişiminde yenilenir; eski oturumların geçersiz kılınmasında kullanılır.
    /// </summary>
    public string SecurityStamp { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>Kurtarma kodlarının SHA-256 özetleri (bellek görünümü).</summary>
    public IReadOnlyCollection<string> RecoveryCodeHashes
    {
        get
        {
            EnsureRecoveryCodesLoaded();
            return _recoveryCodeHashes.AsReadOnly();
        }
    }

    public void UpdateProfile(string firstName, string lastName, bool isActive)
    {
        SetProfile(firstName, lastName);
        IsActive = isActive;
    }

    public void ChangePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
        RotateSecurityStamp();
        ClearPasswordResetChallenge();
    }

    /// <summary>
    /// Rol/izin veya hesap durumu değişince access token'ları geçersiz kılar (#70).
    /// </summary>
    public void InvalidateCredentialStamp() => RotateSecurityStamp();

    /// <summary>Yeni şifre sıfırlama challenge'ı atar (önceki token geçersizleşir).</summary>
    public void SetPasswordResetChallenge(string tokenHash, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAtUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expiry must be in the future.", nameof(expiresAtUtc));

        PasswordResetTokenHash = tokenHash;
        PasswordResetTokenExpiresAt = expiresAtUtc;
    }

    public bool IsPasswordResetChallengeValid(string tokenHash, DateTime utcNow) =>
        PasswordResetTokenHash is not null
        && PasswordResetTokenExpiresAt is not null
        && PasswordResetTokenExpiresAt > utcNow
        && string.Equals(PasswordResetTokenHash, tokenHash, StringComparison.Ordinal);

    public void ClearPasswordResetChallenge()
    {
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAt = null;
    }

    /// <summary>
    /// Admin daveti ile pasif kullanıcı oluşturur. <paramref name="pendingPasswordHash"/> login'i engelleyen
    /// rastgele bir özet olmalıdır; gerçek şifre <see cref="AcceptInvitation"/> ile set edilir.
    /// </summary>
    public static User CreateInvited(
        string email,
        string username,
        string pendingPasswordHash,
        string firstName,
        string lastName,
        string invitationTokenHash,
        DateTime invitationExpiresAtUtc
    )
    {
        var user = new User(email, username, pendingPasswordHash, firstName, lastName);
        user.Deactivate();
        user.SetInvitationChallenge(invitationTokenHash, invitationExpiresAtUtc);
        return user;
    }

    public void SetInvitationChallenge(string tokenHash, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAtUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expiry must be in the future.", nameof(expiresAtUtc));

        InvitationTokenHash = tokenHash;
        InvitationTokenExpiresAt = expiresAtUtc;
    }

    public bool IsInvitationChallengeValid(string tokenHash, DateTime utcNow) =>
        !IsActive
        && InvitationTokenHash is not null
        && InvitationTokenExpiresAt is not null
        && InvitationTokenExpiresAt > utcNow
        && string.Equals(InvitationTokenHash, tokenHash, StringComparison.Ordinal);

    public void ClearInvitationChallenge()
    {
        InvitationTokenHash = null;
        InvitationTokenExpiresAt = null;
    }

    /// <summary>Daveti kabul eder: şifre + profil, aktifleştirme, e-posta doğrulanmış sayılır.</summary>
    public void AcceptInvitation(string passwordHash, string firstName, string lastName)
    {
        if (InvitationTokenHash is null)
            throw new InvalidOperationException("User has no pending invitation.");

        SetProfile(firstName, lastName);
        SetPasswordHash(passwordHash);
        RotateSecurityStamp();
        ClearPasswordResetChallenge();
        ClearInvitationChallenge();
        MarkEmailVerified();
        Activate();
    }

    /// <summary>Yeni e-posta doğrulama challenge'ı atar (önceki token geçersizleşir).</summary>
    public void SetEmailVerificationChallenge(string tokenHash, DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAtUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expiry must be in the future.", nameof(expiresAtUtc));

        EmailVerificationTokenHash = tokenHash;
        EmailVerificationTokenExpiresAt = expiresAtUtc;
        IsEmailVerified = false;
    }

    public bool IsEmailVerificationChallengeValid(string tokenHash, DateTime utcNow) =>
        !IsEmailVerified
        && EmailVerificationTokenHash is not null
        && EmailVerificationTokenExpiresAt is not null
        && EmailVerificationTokenExpiresAt > utcNow
        && string.Equals(EmailVerificationTokenHash, tokenHash, StringComparison.Ordinal);

    /// <summary>E-postayı doğrulanmış işaretler ve challenge alanlarını temizler.</summary>
    public void MarkEmailVerified()
    {
        IsEmailVerified = true;
        ClearEmailVerificationChallenge();
    }

    public void ClearEmailVerificationChallenge()
    {
        EmailVerificationTokenHash = null;
        EmailVerificationTokenExpiresAt = null;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public bool IsLockedOut(DateTime utcNow) =>
        LockoutEnd is { } end && end > utcNow;

    /// <summary>
    /// Başarısız giriş kaydı. Eşik aşılırsa hesabı <paramref name="lockoutDuration"/> kadar kilitler ve sayacı sıfırlar.
    /// </summary>
    public void RegisterFailedAccess(int maxFailedAttempts, TimeSpan lockoutDuration, DateTime utcNow)
    {
        if (maxFailedAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFailedAttempts));
        if (lockoutDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(lockoutDuration));

        AccessFailedCount++;
        if (AccessFailedCount < maxFailedAttempts)
            return;

        LockoutEnd = utcNow.Add(lockoutDuration);
        AccessFailedCount = 0;
    }

    public void ResetAccessFailures()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    /// <summary>
    /// MFA kurulumunu başlatır (henüz etkinleştirmez). Önceki bekleyen kurulum ve kodlar üzerine yazılır.
    /// </summary>
    public void BeginMfaEnrollment(string totpSecretBase32, IReadOnlyList<string> recoveryCodeHashes)
    {
        if (IsMfaEnabled)
            throw new InvalidOperationException("MFA is already enabled. Disable it before re-enrollment.");

        ArgumentException.ThrowIfNullOrWhiteSpace(totpSecretBase32);
        ArgumentNullException.ThrowIfNull(recoveryCodeHashes);
        if (recoveryCodeHashes.Count == 0)
            throw new ArgumentException("At least one recovery code hash is required.", nameof(recoveryCodeHashes));

        var secret = totpSecretBase32.Trim();
        if (secret.Length > TotpSecretMaxLength)
            throw new ArgumentException($"TOTP secret must be at most {TotpSecretMaxLength} characters.", nameof(totpSecretBase32));

        TotpSecret = secret;
        _recoveryCodeHashes = [];
        foreach (var hash in recoveryCodeHashes)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(hash);
            if (hash.Length > RecoveryCodeHashMaxLength)
                throw new ArgumentException($"Recovery code hash must be at most {RecoveryCodeHashMaxLength} characters.");
            _recoveryCodeHashes.Add(hash);
        }

        PersistRecoveryCodes();
    }

    /// <summary>Kurulum TOTP doğrulaması başarılı olduktan sonra MFA'yı açar.</summary>
    public void EnableMfa()
    {
        if (string.IsNullOrWhiteSpace(TotpSecret))
            throw new InvalidOperationException("MFA enrollment has not been started.");

        EnsureRecoveryCodesLoaded();
        if (_recoveryCodeHashes.Count == 0)
            throw new InvalidOperationException("MFA recovery codes are missing.");

        IsMfaEnabled = true;
    }

    /// <summary>Tek kullanımlık kurtarma kodunu tüketir.</summary>
    public bool TryConsumeRecoveryCodeHash(string codeHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);
        EnsureRecoveryCodesLoaded();
        var index = _recoveryCodeHashes.FindIndex(h =>
            string.Equals(h, codeHash, StringComparison.Ordinal));
        if (index < 0)
            return false;

        _recoveryCodeHashes.RemoveAt(index);
        PersistRecoveryCodes();
        return true;
    }

    public void DisableMfa()
    {
        IsMfaEnabled = false;
        TotpSecret = null;
        _recoveryCodeHashes = [];
        RecoveryCodeHashesJson = null;
    }

    private void EnsureRecoveryCodesLoaded()
    {
        if (_recoveryCodeHashes.Count > 0 || string.IsNullOrWhiteSpace(RecoveryCodeHashesJson))
            return;

        _recoveryCodeHashes = JsonSerializer.Deserialize<List<string>>(RecoveryCodeHashesJson) ?? [];
    }

    private void PersistRecoveryCodes() =>
        RecoveryCodeHashesJson = _recoveryCodeHashes.Count == 0
            ? null
            : JsonSerializer.Serialize(_recoveryCodeHashes);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static string NormalizeUsername(string username) => username.Trim();

    private void SetIdentity(string email, string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        Email = NormalizeEmail(email);
        Username = NormalizeUsername(username);
    }

    private void SetProfile(string firstName, string lastName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    private void SetPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }

    private void RotateSecurityStamp() =>
        SecurityStamp = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
}
