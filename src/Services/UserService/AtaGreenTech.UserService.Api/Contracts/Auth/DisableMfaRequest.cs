namespace AtaGreenTech.UserService.Api.Contracts.Auth{

    /// <summary>MFA kapatma istek gövdesi.</summary>
    public sealed record DisableMfaRequest(string Password);
}