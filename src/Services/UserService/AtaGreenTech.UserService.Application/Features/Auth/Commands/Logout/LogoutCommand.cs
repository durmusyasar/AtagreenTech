using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Verilen refresh token ailesini ve access <c>sid</c> denylist kaydını iptal eder.
/// Token yoksa veya zaten iptalse idempotent başarılıdır.
/// </summary>
public sealed record LogoutCommand(string RefreshToken) : ICommand<MessageResult>;
