namespace AtaGreenTech.UserService.Application.Options;

/// <summary>Ürün feature-flag'leri. V1'de OIDC SSO varsayılan kapalıdır.</summary>
public sealed class FeatureFlagsOptions
{
    public const string SectionName = "FeatureFlags";

    /// <summary>OIDC SSO akışlarını açar. V1'de <see langword="false"/>; açılsa bile sağlayıcılar henüz implemente değildir.</summary>
    public bool EnableOidcSso { get; set; }
}
