namespace AtaGreenTech.UserService.Api.Contracts.Auth
{
    /// <summary>Oturumdayken şifre değiştirme istek gövdesi.</summary>
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}