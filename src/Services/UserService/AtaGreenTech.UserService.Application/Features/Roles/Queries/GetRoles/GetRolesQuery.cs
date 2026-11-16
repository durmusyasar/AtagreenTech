using AtaGreenTech.UserService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IQuery<IReadOnlyList<RoleDetailDto>>;
