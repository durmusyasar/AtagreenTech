using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword
{
    /// <summary>
    /// Oturumdaki kullanıcının parolasını değiştirir; tüm refresh/access oturumlarını sonlandırır.
    /// Yanlış mevcut parola → <c>AUTH_PASSWORD_MISMATCH</c> (401 değil — istemci oturum düşürmesin).
    /// </summary>
    public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand<MessageResult>;
}