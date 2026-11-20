using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh
{
    /// <summary>
    /// Tek kullanımlık refresh rotation. Revoke edilmiş token yeniden sunulursa aile iptal edilir (reuse detection).
    /// </summary>
    public sealed record RefreshTokenCommand(
        string RefreshToken,
        string? ClientIp = null,
        string? DeviceInfo = null
    ) : ICommand<AuthAttemptResult>;
}