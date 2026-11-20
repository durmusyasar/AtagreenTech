using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Queries.ListTenants;

public sealed record ListTenantsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IQuery<TenantListResponse>;
