namespace AtaGreenTech.SharedKernel.Localization;

/// <summary>
/// Aktif <see cref="System.Globalization.CultureInfo.CurrentUICulture"/> üzerinden hata metni üretir (#71).
/// Kaynak bulunamazsa <paramref name="fallback"/> veya ham <paramref name="errorCode"/> döner.
/// </summary>
public interface IErrorLocalizer
{
    string Localize(string errorCode, params object[] args);

    string Localize(string errorCode, string? fallback, params object[] args);
}
