using AtaGreenTech.SharedKernel.Security;

namespace AtaGreenTech.IntegrationTests.Infrastructure;

/// <summary>
/// UserService ↔ StockService E2E testlerinde paylaşılan JWT imza sözleşmesi.
/// Production değerleriyle aynı Issuer/Audience; SigningKey yalnızca test izolasyonu için.
/// </summary>
public static class SharedJwtContract
{
    public const string SigningKey = "AtaGreenTechIntegrationTestSuperSecureJwtKey2026!";
    public const string Issuer = JwtContract.Issuer;
    public const string Audience = JwtContract.Audience;
    public const string TenantSubdomain = "e2e";
    public const string TenantHost = "e2e.omnisync.localhost";
    public const string AdminEmail = "admin@omnisync.com";
    public const string AdminPassword = "Admin123!";
    public const string ViewerEmail = "viewer@test.com";
    public const string ViewerPassword = "StrongPass123!";
}
