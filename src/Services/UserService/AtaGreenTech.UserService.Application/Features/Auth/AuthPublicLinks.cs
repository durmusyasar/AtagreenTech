using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Options;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Tenant subdomain’li public uygulama linkleri (verify / reset / invite).
    /// Üretilen URL e-posta gövdesine gider; kullanıcıya API mesajı değildir.
    /// Geçersiz girdilerde <see cref="ErrorCodes"/> taşınır (#71).
    /// </summary>
    internal static class AuthPublicLinks
    {
        /// <summary>
        /// <c>{PublicAppBaseUrl}</c> (+ varsa <c>{tenant}.{host}</c>) / <paramref name="path"/>?token=…
        /// <c>localhost</c> gibi tek etiketli host’larda subdomain eklenmez.
        /// </summary>
        public static string Build(AuthOptions options, ICurrentTenant tenant, string path, string tokenQueryValue)
        {
            if (string.IsNullOrWhiteSpace(options.PublicAppBaseUrl))
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(options.PublicAppBaseUrl));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(path));
            if (string.IsNullOrWhiteSpace(tokenQueryValue))
                throw new ArgumentException(ErrorCodes.ValidationRequired, nameof(tokenQueryValue));

            if (!Uri.TryCreate(options.PublicAppBaseUrl.Trim(), UriKind.Absolute, out var baseUri)
                || (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException(ErrorCodes.ValidationInvalid, nameof(options.PublicAppBaseUrl));

            var authority = ResolveAuthority(baseUri, tenant.Identifier);
            var pathPart = path.StartsWith('/') ? path.Trim() : "/" + path.Trim();
            return $"{authority}{pathPart}?token={Uri.EscapeDataString(tokenQueryValue)}";
        }

        /// <summary>
        /// Tenant identifier varsa ve host çok etiketliyse subdomain önekler;
        /// zaten önekliyse tekrar eklemez.
        /// </summary>
        private static string ResolveAuthority(Uri baseUri, string? tenantIdentifier)
        {
            var identifier = tenantIdentifier?.Trim();
            if (string.IsNullOrWhiteSpace(identifier))
                return baseUri.GetLeftPart(UriPartial.Authority);

            var hostLabels = baseUri.Host.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (hostLabels.Length < 2)
                return baseUri.GetLeftPart(UriPartial.Authority);

            var prefix = identifier + ".";
            if (baseUri.Host.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return baseUri.GetLeftPart(UriPartial.Authority);

            var builder = new UriBuilder(baseUri) { Host = prefix + baseUri.Host };
            return builder.Uri.GetLeftPart(UriPartial.Authority);
        }
    }
}