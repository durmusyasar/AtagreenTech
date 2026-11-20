using System.Globalization;
using AtaGreenTech.SharedKernel.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace AtaGreenTech.UserService.Tests.Localization;

public sealed class ErrorLocalizerTests
{
    [Theory]
    [InlineData("tr", ErrorCodes.AuthInvalidCredentials, "E-posta adresi veya şifre hatalı.")]
    [InlineData("en", ErrorCodes.AuthInvalidCredentials, "Invalid email address or password.")]
    [InlineData("tr", ErrorCodes.PermissionDenied, "Bu işlemi gerçekleştirmek için gerekli izne sahip değilsiniz.")]
    [InlineData("en", ErrorCodes.ModuleNotSubscribed, "This module is not included in your active plan.")]
    public void Localize_uses_accept_language_culture(string culture, string code, string expected)
    {
        var localizer = CreateLocalizer();
        using var _ = new CultureScope(culture);

        Assert.Equal(expected, localizer.Localize(code));
    }

    [Fact]
    public void Localize_formats_account_locked_minutes()
    {
        var localizer = CreateLocalizer();
        using var _ = new CultureScope("en");

        var message = localizer.Localize(ErrorCodes.AuthAccountLocked, 15);

        Assert.Equal(
            "Your account is temporarily locked due to multiple failed attempts. Please try again in 15 minutes.",
            message
        );
    }

    [Fact]
    public void Localize_falls_back_to_code_when_key_missing()
    {
        var localizer = CreateLocalizer();
        using var _ = new CultureScope("tr");

        Assert.Equal("UNKNOWN_KEY_XYZ", localizer.Localize("UNKNOWN_KEY_XYZ"));
    }

    [Fact]
    public void Unsupported_culture_still_resolves_via_default_resources()
    {
        var localizer = CreateLocalizer();
        // RequestLocalization falls back to tr at middleware; here we only assert resource lookup
        // does not throw for an odd CurrentUICulture.
        using var _ = new CultureScope("es-ES");

        var message = localizer.Localize(ErrorCodes.AuthInvalidCredentials);
        Assert.False(string.IsNullOrWhiteSpace(message));
        Assert.NotEqual(ErrorCodes.AuthInvalidCredentials, message);
    }

    private static IErrorLocalizer CreateLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtaGreenLocalization();
        return services.BuildServiceProvider().GetRequiredService<IErrorLocalizer>();
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _prevCulture;
        private readonly CultureInfo _prevUiCulture;

        public CultureScope(string culture)
        {
            _prevCulture = CultureInfo.CurrentCulture;
            _prevUiCulture = CultureInfo.CurrentUICulture;
            var info = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = info;
            CultureInfo.CurrentUICulture = info;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _prevCulture;
            CultureInfo.CurrentUICulture = _prevUiCulture;
        }
    }
}
