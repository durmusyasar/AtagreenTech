using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AtaGreenTech.UserService.Infrastructure.Email;

/// <summary>
/// SMTP/SendGrid yerine doğrulama linkini loglayan geliştirme göndericisi.
/// </summary>
internal sealed class LoggingEmailVerificationSender(ILogger<LoggingEmailVerificationSender> logger)
    : IEmailVerificationSender
{
    public Task SendAsync(string email, string verificationUrl, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Email verification link for {Email}: {VerificationUrl}",
            email,
            verificationUrl
        );
        return Task.CompletedTask;
    }
}
