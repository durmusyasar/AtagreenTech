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

    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

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

    /// <summary>
    /// Parola veya kritik kimlik değişiminde yenilenir; eski oturumların geçersiz kılınmasında kullanılır.
    /// </summary>
    public string SecurityStamp { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public void UpdateProfile(string firstName, string lastName, bool isActive)
    {
        SetProfile(firstName, lastName);
        IsActive = isActive;
    }

    public void ChangePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
        RotateSecurityStamp();
    }

    public void MarkEmailVerified() => IsEmailVerified = true;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

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
