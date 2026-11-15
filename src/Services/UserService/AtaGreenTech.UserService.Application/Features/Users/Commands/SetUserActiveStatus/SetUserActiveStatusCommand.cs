using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.SetUserActiveStatus;

public sealed record SetUserActiveStatusCommand(Guid Id, bool IsActive) : ICommand<MessageResult>;
