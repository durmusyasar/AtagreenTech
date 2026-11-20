namespace AtaGreenTech.UserService.Api.Contracts.Auth{

    /// <summary>E-posta doğrulama istek gövdesi.</summary>
    public sealed record VerifyEmailRequest(string Token);
}
