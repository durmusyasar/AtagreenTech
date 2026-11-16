namespace AtaGreenTech.UserService.Application.Options;

/// <summary>
/// OIDC sağlayıcı yer tutucuları (<c>OidcProviders</c>). V2'de Google / Microsoft (Entra ID) bağlanacak.
/// Bölüm altındaki her child key bir sağlayıcı adıdır (Google, Microsoft, …).
/// </summary>
public sealed class OidcOptions
{
    public const string SectionName = "OidcProviders";

    public Dictionary<string, OidcProviderConfig> Providers { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetProvider(string provider, out OidcProviderConfig config)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        return Providers.TryGetValue(provider.Trim(), out config!);
    }
}

/// <summary>Tek bir OIDC sağlayıcısının yapılandırması.</summary>
public sealed class OidcProviderConfig
{
    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = string.Empty;
}
