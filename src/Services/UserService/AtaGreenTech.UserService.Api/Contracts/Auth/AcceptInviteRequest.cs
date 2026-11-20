namespace AtaGreenTech.UserService.Api.Contracts.Auth
{
    /// <summary>Davet kabul istek gövdesi.</summary>
    public sealed record AcceptInviteRequest(string Token, string Password, string FirstName, string LastName);
}