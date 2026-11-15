using AtaGreenTech.UserService.Application.Features.Users;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    IReadOnlyList<string>? Roles,
    string? ClientIp = null
) : ICommand<UserWriteResult>;
