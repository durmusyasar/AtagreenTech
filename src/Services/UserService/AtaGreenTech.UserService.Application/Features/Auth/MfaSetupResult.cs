namespace AtaGreenTech.UserService.Application.Features.Auth;

public sealed record MfaSetupResult(bool Succeeded, MfaSetupResponse? Setup, string? Message)
{
    public static MfaSetupResult Ok(MfaSetupResponse setup) => new(true, setup, null);

    public static MfaSetupResult Fail(string message) => new(false, null, message);
}
