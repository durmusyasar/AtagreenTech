using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa
{
    /// <summary>
    /// Kurulum TOTP kodunu doğrular ve MFA’yı etkinleştirir.
    /// Önkoşul: <c>SetupMfa</c> ile enrollment; zaten açıksa <c>AUTH_MFA_ALREADY_ENABLED</c>.
    /// </summary>
    public sealed record VerifyMfaCommand(string Code) : ICommand<MessageResult>;
}