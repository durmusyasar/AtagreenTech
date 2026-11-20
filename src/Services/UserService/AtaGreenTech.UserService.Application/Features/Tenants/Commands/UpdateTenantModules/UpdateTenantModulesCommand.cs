using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Commands.UpdateTenantModules;

public sealed record UpdateTenantModulesCommand(
    Guid TenantId,
    IReadOnlyList<TenantModuleItemRequest> Modules
) : ICommand;
