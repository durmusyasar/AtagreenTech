using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa
{
    /// <summary>
    /// Mevcut parola ile MFA’yı kapatır; secret ve kurtarma kodlarını temizler.
    /// Zaten kapalıysa → <c>AUTH_MFA_ALREADY_DISABLED</c>; yanlış parola → <c>AUTH_PASSWORD_MISMATCH</c> (401 değil).
    /// </summary>
    public sealed record DisableMfaCommand(string Password) : ICommand<MessageResult>;
}