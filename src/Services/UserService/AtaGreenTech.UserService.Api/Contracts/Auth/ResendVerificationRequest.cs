namespace AtaGreenTech.UserService.Api.Contracts.Auth{

    /// <summary>Doğrulama e-postasını yeniden gönderme istek gövdesi.</summary>
    public sealed record ResendVerificationRequest(string Email);
}
