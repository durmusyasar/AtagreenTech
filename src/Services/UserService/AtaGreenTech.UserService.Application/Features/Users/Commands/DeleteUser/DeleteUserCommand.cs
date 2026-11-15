using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : ICommand<MessageResult>;
