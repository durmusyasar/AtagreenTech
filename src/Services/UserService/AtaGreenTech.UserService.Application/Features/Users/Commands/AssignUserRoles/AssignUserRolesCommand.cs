using AtaGreenTech.UserService.Application.Features.Users;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.AssignUserRoles;

public sealed record AssignUserRolesCommand(
    Guid UserId,
    IReadOnlyList<string> Roles,
    string? ClientIp = null
) : ICommand<UserWriteResult>;
