namespace AtaGreenTech.UserService.Application.Options;

/// <summary>
/// Dahili tenant seed HTTP uç noktası için paylaşılan anahtar (<c>InternalProvisioning:ApiKey</c>).
/// Boş bırakılırsa endpoint tüm istekleri 401 döner.
/// </summary>
public sealed class InternalProvisioningOptions
{
    public const string SectionName = "InternalProvisioning";

    public string ApiKey { get; set; } = string.Empty;
}
