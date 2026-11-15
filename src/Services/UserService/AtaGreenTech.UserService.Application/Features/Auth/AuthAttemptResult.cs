namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Auth komutlarının başarı/hata sonucu. Reuse detection gibi DB'ye yazılması gereken güvenlik
/// işlemlerinde exception fırlatılmaz; aksi halde OmniSync transaction rollback aile iptalini geri alır.
/// </summary>
public sealed record AuthAttemptResult(AuthResponse? Response, string? ErrorMessage)
{
    public bool IsSuccess => Response is not null;

    public static AuthAttemptResult Ok(AuthResponse response) => new(response, null);

    public static AuthAttemptResult Fail(string errorMessage) => new(null, errorMessage);
}
