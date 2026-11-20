namespace AtaGreenTech.UserService.Application.Interfaces
{
    /// <summary>
    /// Google Authenticator / Authy uyumlu TOTP üretimi ve doğrulaması (RFC 6238).
    /// </summary>
    public interface ITotpService
    {
        /// <summary>Yeni Base32 gizli anahtar üretir (Authenticator shared key).</summary>
        string GenerateSecret();

        /// <summary>
        /// QR / manuel giriş için <c>otpauth://totp/…</c> URI üretir.
        /// </summary>
        /// <param name="email">Hesap etiketi (kullanıcı e-postası).</param>
        /// <param name="secretBase32">Base32 shared key.</param>
        /// <param name="issuer">Authenticator’da görünen uygulama adı.</param>
        string BuildAuthenticatorUri(string email, string secretBase32, string issuer);

        /// <summary>
        /// 6 haneli TOTP kodunu ±1 zaman penceresiyle doğrular.
        /// Geçersiz/boş girdi → <see langword="false"/> (exception yok).
        /// </summary>
        /// <param name="secretBase32">Base32 shared key.</param>
        /// <param name="code">Kullanıcının girdiği kod (boşluk/ayraç toleranslı).</param>
        bool VerifyCode(string secretBase32, string code);
    }
}