using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Queries.GetTenantModules;

internal sealed class GetTenantModulesQueryHandler(ITenantManagementAdmin admin)
    : IQueryHandler<GetTenantModulesQuery, IReadOnlyList<TenantModuleItemDto>>
{
    public Task<IReadOnlyList<TenantModuleItemDto>> Handle(
        GetTenantModulesQuery request,
        CancellationToken cancellationToken
    ) => admin.GetTenantModulesAsync(request.TenantId, request.PreferredLanguage, cancellationToken);
}
