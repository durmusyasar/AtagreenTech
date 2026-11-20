namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Auth komutlarının başarı/hata sonucu. Lockout sayacı, recovery code tüketimi ve refresh reuse
    /// detection gibi DB yazımlarında exception fırlatılmaz; aksi halde OmniSync transaction rollback eder.
    /// Hatalarda kullanıcı metni taşınmaz; <see cref="ErrorCode"/> (+ <see cref="ErrorArgs"/>) API sınırında lokalize edilir (#71).
    /// </summary>
    public sealed record AuthAttemptResult(
        AuthResponse? Response,
        string? ErrorCode = null,
        int StatusCode = 401,
        bool MfaRequired = false,
        IReadOnlyList<object>? ErrorArgs = null
    )
    {
        /// <summary>Access/refresh üretildiyse başarılıdır.</summary>
        public bool IsSuccess => Response is not null;

        /// <summary>Başarılı oturum açma / token yenileme sonucu.</summary>
        public static AuthAttemptResult Ok(AuthResponse response) => new(response);

        /// <summary>
        /// Yalnızca <paramref name="errorCode"/>; metin API'de <c>IErrorLocalizer</c> ile üretilir.
        /// </summary>
        public static AuthAttemptResult Fail(string errorCode, int statusCode = 401) =>
            new(null, errorCode, statusCode);

        /// <summary>
        /// Format argümanlı hata (ör. kilit süresi dakikası).
        /// İkinci parametre her zaman HTTP <paramref name="statusCode"/> olmalıdır; argüman üçüncü sıradadır.
        /// </summary>
        public static AuthAttemptResult Fail(string errorCode, int statusCode, object errorArg) =>
            new(null, errorCode, statusCode, MfaRequired: false, ErrorArgs: [errorArg]);

        /// <summary>MFA gerekli; <see cref="AuthErrorCodes.MfaRequired"/> + <see cref="MfaRequired"/>.</summary>
        public static AuthAttemptResult RequireMfa() =>
            new(
                null,
                AuthErrorCodes.MfaRequired,
                StatusCode: 401,
                MfaRequired: true
            );
    }
}