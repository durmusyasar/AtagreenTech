using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;

/// <summary>
/// Doğrulama e-postasını yeniden gönderir. Kullanıcı yoksa veya zaten doğrulanmışsa sessizce başarılı döner
/// (e-posta enumeration önleme).
/// </summary>
public sealed record ResendVerificationCommand(string Email) : ICommand<MessageResult>;
