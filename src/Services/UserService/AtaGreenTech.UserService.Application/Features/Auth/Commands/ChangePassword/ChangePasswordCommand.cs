using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword)
    : ICommand<MessageResult>;
