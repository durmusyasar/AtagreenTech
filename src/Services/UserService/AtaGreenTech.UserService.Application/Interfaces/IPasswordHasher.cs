namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>
/// Parola özetleme ve doğrulama. Uygulama katmanı PBKDF2 ayrıntılarından bağımsız kalır.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Salt + PBKDF2 özetini <c>salt.hash</c> (Base64) biçiminde üretir.</summary>
    string Hash(string password);

    /// <summary>Verilen parolanın saklanan özetle eşleşip eşleşmediğini sabit zamanda doğrular.</summary>
    bool Verify(string password, string storedHash);
}
