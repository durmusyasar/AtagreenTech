using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;

public sealed record AcceptInviteCommand(string Token, string Password, string FirstName, string LastName)
    : ICommand<MessageResult>;
