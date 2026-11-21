using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Queries.ListTenants;

internal sealed class ListTenantsQueryHandler(ITenantManagementAdmin admin)
    : IQueryHandler<ListTenantsQuery, TenantListResponse>
{
    public Task<TenantListResponse> Handle(ListTenantsQuery request, CancellationToken cancellationToken) =>
        admin.ListTenantsAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.SortBy,
            request.SortDir,
            cancellationToken
        );
}
