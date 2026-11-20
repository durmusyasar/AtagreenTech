namespace AtaGreenTech.UserService.Api.Contracts.Auth{

/// <summary>Login istek gövdesi. MFA açıksa <see cref="TotpCode"/> veya <see cref="RecoveryCode"/> gerekir.</summary>
    public sealed record LoginRequest(string Email, string Password, string? TotpCode = null, string? RecoveryCode = null);
}
