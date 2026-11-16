using AtaGreenTech.UserService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Roles.Queries.GetPermissions;

public sealed record GetPermissionsQuery : IQuery<IReadOnlyList<PermissionDto>>;
