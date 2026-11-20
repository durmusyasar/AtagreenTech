using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword
{
    /// <summary>
    /// Şifre sıfırlama jetonunu tüketir, yeni parolayı yazar ve tüm oturumları sonlandırır.
    /// Geçersiz/süresi dolmuş jeton → <c>AUTH_RESET_TOKEN_INVALID</c>.
    /// </summary>
    public sealed record ResetPasswordCommand(
        string Token,
        string NewPassword,
        string? ClientIp = null
    ) : ICommand<MessageResult>;
}