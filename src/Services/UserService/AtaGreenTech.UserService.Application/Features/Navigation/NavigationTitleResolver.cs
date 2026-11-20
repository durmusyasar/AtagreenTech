using AtaGreenTech.UserService.Application.Common;

namespace AtaGreenTech.UserService.Application.Features.Navigation;

/// <summary>Accept-Language → varsayılan dil → kod fallback.</summary>
public static class NavigationTitleResolver
{
    public static string Resolve(
        IReadOnlyDictionary<string, string> translations,
        string? preferredLanguage,
        string? defaultLanguage,
        string fallbackCode
    )
    {
        if (translations.Count == 0)
            return fallbackCode;

        if (!string.IsNullOrWhiteSpace(preferredLanguage)
            && translations.TryGetValue(Normalize(preferredLanguage), out var preferred)
            && !string.IsNullOrWhiteSpace(preferred))
            return preferred;

        if (!string.IsNullOrWhiteSpace(defaultLanguage)
            && translations.TryGetValue(Normalize(defaultLanguage), out var def)
            && !string.IsNullOrWhiteSpace(def))
            return def;

        return StringHelpers.FirstNonEmpty(translations.Values) ?? fallbackCode;
    }

    public static string Normalize(string languageCode)
    {
        var trimmed = languageCode.Trim();
        var primary = trimmed.Split(',', 2)[0].Trim();
        var tag = primary.Split(';', 2)[0].Trim();
        var dash = tag.IndexOf('-');
        return (dash > 0 ? tag[..dash] : tag).ToLowerInvariant();
    }
}
