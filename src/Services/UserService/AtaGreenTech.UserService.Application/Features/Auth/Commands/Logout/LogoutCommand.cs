using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Verilen refresh token'ı iptal eder. Token yoksa veya zaten iptalse idempotent başarılıdır.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand;
