using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    IReadOnlyList<TenantModuleItemRequest>? Modules = null,
    string? AdminEmail = null,
    string? AdminFirstName = null,
    string? AdminLastName = null,
    string? AdminPassword = null
) : ICommand<CreateTenantResult>;
