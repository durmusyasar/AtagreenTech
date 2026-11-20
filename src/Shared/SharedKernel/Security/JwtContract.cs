namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// UserService ↔ Stock (ve diğer API'ler) arasında ortak JWT sözleşmesi.
/// OmniSync <c>JwtSettings</c> bu değerlerle hizalanmalıdır: imza <c>SigningKey</c> (HS256) veya
/// RS256 anahtar çifti; claim adları OmniSync <c>TokenClaimTypes</c> ile aynıdır.
/// </summary>
public static class JwtContract
{
    /// <summary>Access token üreten servis (UserService). Doğrulayan API'ler <c>JwtSettings:Issuer</c> olarak bunu kullanır.</summary>
    public const string Issuer = "AtaGreenTech.UserService";

    /// <summary>Tüm AtaGreen istemci uygulamaları.</summary>
    public const string Audience = "AtaGreenTech.Apps";

    /// <summary>Konfigürasyon bölümü adı (OmniSync <c>JwtOptions.SectionName</c>).</summary>
    public const string SettingsSection = "JwtSettings";

    /// <summary>Varsayılan access token ömrü (dakika). Üretimde UserService mint ayarı bunu kullanır.</summary>
    public const int DefaultAccessTokenMinutes = 60;

    public static class Claims
    {
        public const string Subject = "sub";
        public const string Email = "email";
        public const string Role = "role";
        public const string TenantId = "tenant_id";

        /// <summary>Granüler RBAC izni (ör. <c>users.read</c>). OmniSync TokenClaimTypes'ta yoktur; yerel sözleşmedir.</summary>
        public const string Permission = "permission";

        /// <summary>Refresh token ailesi / oturum kimliği (#70). Logout ve session revoke bu değerle access'i iptal eder.</summary>
        public const string SessionId = "sid";

        /// <summary><c>User.SecurityStamp</c> anlık görüntüsü (#70). Stamp dönünce eski access reddedilir.</summary>
        public const string SecurityStamp = "sst";
    }
}


