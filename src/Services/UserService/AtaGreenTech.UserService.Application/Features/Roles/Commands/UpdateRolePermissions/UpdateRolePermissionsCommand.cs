using AtaGreenTech.UserService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Roles.Commands.UpdateRolePermissions;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyList<string> Permissions,
    string? ClientIp = null
) : ICommand<RoleWriteResult>;

public sealed record RoleWriteResult(bool Succeeded, string Message, RoleDetailDto? Role = null)
{
    public static RoleWriteResult Ok(RoleDetailDto role, string message = "") => new(true, message, role);

    public static RoleWriteResult Fail(string message) => new(false, message);
}
