namespace AtaGreenTech.UserService.Api.Contracts.Auth
{
    /// <summary>MFA kurulum doğrulama istek gövdesi.</summary>
    public sealed record VerifyMfaRequest(string Code);
}