namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// MFA kurulum komutu sonucu. Hatalarda yalnızca <see cref="ErrorCode"/> taşır;
    /// yerelleştirme API sınırında yapılır (#71).
    /// </summary>
    public sealed record MfaSetupResult(
        bool Succeeded,
        MfaSetupResponse? Setup,
        string? ErrorCode = null,
        IReadOnlyList<object>? ErrorArgs = null)
    {
        public static MfaSetupResult Ok(MfaSetupResponse setup) => new(true, setup);

        public static MfaSetupResult Fail(string errorCode, params object[] args) =>
            new(false, null, errorCode, args.Length == 0 ? null : args);
    }
}