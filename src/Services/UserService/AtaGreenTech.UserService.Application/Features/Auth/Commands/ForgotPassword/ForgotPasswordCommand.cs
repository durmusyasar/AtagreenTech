using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Şifre sıfırlama e-postası ister. Kullanıcı yoksa bile generic başarı döner (enumeration önleme).
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : ICommand<MessageResult>;
