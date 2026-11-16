using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Email/şifre ile giriş. MFA açıksa TOTP veya kurtarma kodu gerekir; aksi halde <c>AUTH_MFA_REQUIRED</c> döner.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? ClientIp = null,
    string? DeviceInfo = null,
    string? TotpCode = null,
    string? RecoveryCode = null
) : ICommand<AuthAttemptResult>;
