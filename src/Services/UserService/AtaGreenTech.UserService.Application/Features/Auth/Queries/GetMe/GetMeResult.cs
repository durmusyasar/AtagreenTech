namespace AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;

/// <summary>
/// <c>GET /auth/me</c> sonucu. Hatalarda yalnızca <see cref="ErrorCode"/> taşır; yerelleştirme API’de (#71).
/// </summary>
public sealed record GetMeResult(
    UserMeResponse? Me,
    string? ErrorCode = null,
    int StatusCode = 401
)
{
    public bool Succeeded => Me is not null;

    public static GetMeResult Ok(UserMeResponse me) => new(me);

    public static GetMeResult Fail(string errorCode, int statusCode = 401) =>
        new(null, errorCode, statusCode);
}
