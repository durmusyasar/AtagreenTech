using AtaGreenTech.UserService.Application.Features.Users;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    bool IsActive = true
) : ICommand<UserWriteResult>;
