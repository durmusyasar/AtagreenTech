namespace AtaGreenTech.UserService.Api.Contracts.Auth{

    /// <summary>Logout istek gövdesi.</summary>
    public sealed record LogoutRequest(string RefreshToken);
}
