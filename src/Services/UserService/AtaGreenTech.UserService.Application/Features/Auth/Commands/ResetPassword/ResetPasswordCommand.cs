using OmniSync.Core.Application.Messaging;
using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string? ClientIp = null
) : ICommand<MessageResult>;
