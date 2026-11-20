using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Queries.GetTenantModules;

public sealed record GetTenantModulesQuery(Guid TenantId, string? PreferredLanguage = null)
    : IQuery<IReadOnlyList<TenantModuleItemDto>>;
