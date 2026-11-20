using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.SetupMfa
{
    /// <summary>
    /// Oturumdaki kullanıcı için TOTP secret + kurtarma kodları üretir (henüz etkinleştirmez).
    /// MFA zaten açıksa → <c>AUTH_MFA_ALREADY_ENABLED</c>. Doğrulama <c>VerifyMfa</c> ile tamamlanır.
    /// </summary>
    public sealed record SetupMfaCommand : ICommand<MfaSetupResult>;
}