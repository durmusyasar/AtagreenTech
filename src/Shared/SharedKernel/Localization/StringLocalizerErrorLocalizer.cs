using Microsoft.Extensions.Localization;

namespace AtaGreenTech.SharedKernel.Localization;

internal sealed class StringLocalizerErrorLocalizer(IStringLocalizer<SharedResource> localizer)
    : IErrorLocalizer
{
    public string Localize(string errorCode, params object[] args) =>
        Localize(errorCode, fallback: null, args);

    public string Localize(string errorCode, string? fallback, params object[] args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        var localized = args.Length == 0
            ? localizer[errorCode]
            : localizer[errorCode, args];

        if (!localized.ResourceNotFound && !string.IsNullOrWhiteSpace(localized.Value))
            return localized.Value;

        if (!string.IsNullOrWhiteSpace(fallback))
            return fallback;

        return errorCode;
    }
}
