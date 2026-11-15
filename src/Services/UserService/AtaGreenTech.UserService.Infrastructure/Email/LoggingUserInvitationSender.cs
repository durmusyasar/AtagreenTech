using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AtaGreenTech.UserService.Infrastructure.Email;

internal sealed class LoggingUserInvitationSender(ILogger<LoggingUserInvitationSender> logger)
    : IUserInvitationSender
{
    public Task SendAsync(string email, string inviteUrl, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("User invitation link for {Email}: {InviteUrl}", email, inviteUrl);
        return Task.CompletedTask;
    }
}
