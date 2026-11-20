namespace AtaGreenTech.UserService.Api.Contracts.Auth{

    /// <summary>Şifre sıfırlama istek gövdesi.</summary>
    public sealed record ResetPasswordRequest(string Token, string NewPassword);
}
