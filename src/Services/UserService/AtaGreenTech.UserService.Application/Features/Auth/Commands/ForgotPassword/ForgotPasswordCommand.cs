using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword
{
    /// <summary>
    /// Şifre sıfırlama e-postası ister. Kullanıcı yoksa veya pasifse aynı başarı kodunu döner
    /// (e-posta enumeration önleme).
    /// </summary>
    public sealed record ForgotPasswordCommand(string Email) : ICommand<MessageResult>;
}