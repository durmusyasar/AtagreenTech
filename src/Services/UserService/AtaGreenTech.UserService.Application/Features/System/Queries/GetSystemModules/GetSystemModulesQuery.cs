using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemModules;

public sealed record GetSystemModulesQuery : IQuery<IReadOnlyList<SystemModuleDto>>;
