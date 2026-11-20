namespace AtaGreenTech.UserService.Application.Interfaces
{
    /// <summary>
    /// Parola özetleme ve doğrulama. Uygulama katmanı PBKDF2 ayrıntılarından bağımsız kalır.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>Salt + PBKDF2 özetini <c>v1.salt.hash</c> (Base64) biçiminde üretir.</summary>
        string Hash(string password);

        /// <summary>
        /// Verilen parolanın saklanan özetle eşleşip eşleşmediğini sabit zamanda doğrular
        /// (legacy <c>salt.hash</c> biçimi dahil).
        /// </summary>
        bool Verify(string password, string storedHash);
    }
}