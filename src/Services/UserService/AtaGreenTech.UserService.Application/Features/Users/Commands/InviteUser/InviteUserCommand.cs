using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.InviteUser;

public sealed record InviteUserCommand(string Email, string FirstName, string LastName, string Role)
    : ICommand<MessageResult>;
