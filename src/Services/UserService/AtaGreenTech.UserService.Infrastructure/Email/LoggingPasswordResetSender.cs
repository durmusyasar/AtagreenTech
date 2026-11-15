using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AtaGreenTech.UserService.Infrastructure.Email;

internal sealed class LoggingPasswordResetSender(ILogger<LoggingPasswordResetSender> logger)
    : IPasswordResetSender
{
    public Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Password reset link for {Email}: {ResetUrl}", email, resetUrl);
        return Task.CompletedTask;
    }
}
