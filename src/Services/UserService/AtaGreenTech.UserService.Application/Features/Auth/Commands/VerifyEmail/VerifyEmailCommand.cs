using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail
{
    /// <summary>
    /// E-posta doğrulama jetonunu tüketir. Geçersiz/süresi dolmuş jeton → <c>AUTH_VERIFY_TOKEN_INVALID</c>.
    /// </summary>
    public sealed record VerifyEmailCommand(string Token) : ICommand<MessageResult>;
}