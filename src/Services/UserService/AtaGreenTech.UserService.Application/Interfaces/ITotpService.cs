namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Google Authenticator / Authy uyumlu TOTP üretimi ve doğrulaması.</summary>
public interface ITotpService
{
    /// <summary>Yeni Base32 gizli anahtar üretir.</summary>
    string GenerateSecret();

    /// <summary><c>otpauth://</c> URI (QR kod için).</summary>
    string BuildAuthenticatorUri(string email, string secretBase32, string issuer);

    /// <summary>6 haneli TOTP kodunu RFC penceresiyle doğrular.</summary>
    bool VerifyCode(string secretBase32, string code);
}
