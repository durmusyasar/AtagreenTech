namespace AtaGreenTech.UserService.Api.Contracts.Auth{

    /// <summary>Şifre unutma istek gövdesi.</summary>
    public sealed record ForgotPasswordRequest(string Email);
}
