using AtaGreenTech.UserService.Application.Options;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Tenant subdomain'li public uygulama linkleri (verify / reset).</summary>
internal static class AuthPublicLinks
{
    public static string Build(AuthOptions options, ICurrentTenant tenant, string path, string tokenQueryValue)
    {
        var baseUrl = options.PublicAppBaseUrl.TrimEnd('/');
        var tenantId = tenant.Identifier;

        if (!string.IsNullOrWhiteSpace(tenantId)
            && Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            && uri.Host.Split('.').Length >= 2)
        {
            var builder = new UriBuilder(uri) { Host = $"{tenantId}.{uri.Host}" };
            baseUrl = builder.Uri.GetLeftPart(UriPartial.Authority);
        }

        var pathPart = path.StartsWith('/') ? path : "/" + path;
        return $"{baseUrl}{pathPart}?token={Uri.EscapeDataString(tokenQueryValue)}";
    }
}
