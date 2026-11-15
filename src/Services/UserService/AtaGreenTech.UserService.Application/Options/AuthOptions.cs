using AtaGreenTech.SharedKernel.Security;

namespace AtaGreenTech.UserService.Application.Options;

/// <summary>
/// Access / refresh token ömürleri ve boş tenant için ilk admin bootstrap ayarları.
/// </summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Access token geçerlilik süresi (dakika). Varsayılan <see cref="JwtContract.DefaultAccessTokenMinutes"/>.</summary>
    public int AccessTokenMinutes { get; set; } = JwtContract.DefaultAccessTokenMinutes;

    /// <summary>Refresh token geçerlilik süresi (gün).</summary>
    public int RefreshTokenDays { get; set; } = 14;

    /// <summary>E-posta doğrulama token'ının geçerlilik süresi (saat).</summary>
    public int EmailVerificationHours { get; set; } = 24;

    /// <summary>Şifre sıfırlama token'ının geçerlilik süresi (saat).</summary>
    public int PasswordResetHours { get; set; } = 1;

    /// <summary>Kullanıcı davet token'ının geçerlilik süresi (gün).</summary>
    public int InvitationDays { get; set; } = 3;

    /// <summary>
    /// Doğrulama / sıfırlama / davet linkinin host'u (ör. <c>https://omnisync.com</c>). Tenant subdomain'i gönderimde eklenir.
    /// </summary>
    public string PublicAppBaseUrl { get; set; } = "https://omnisync.com";

    public BootstrapAdminOptions Bootstrap { get; set; } = new();
}

/// <summary>
/// Tenant DB'sinde hiç kullanıcı yokken oluşturulan ilk Admin hesabı. Parola üretimde secret/env ile verilmelidir.
/// </summary>
public sealed class BootstrapAdminOptions
{
    public string Email { get; set; } = "admin@omnisync.com";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = "System";

    public string LastName { get; set; } = "Admin";
}
