using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Email/şifre ile giriş. Tenant boşsa bootstrap admin oluşturulur; ardından access + refresh token döner.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? ClientIp = null,
    string? DeviceInfo = null
) : ICommand<AuthResponse>;
