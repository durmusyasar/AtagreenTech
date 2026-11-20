using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;

namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Platform tenant listesi / oluşturma / modül aboneliği (master DB).</summary>
public interface ITenantManagementAdmin
{
    Task<TenantListResponse> ListTenantsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    );

    Task<CreateTenantResult> CreateTenantAsync(
        CreateTenantRequest request,
        Guid? actorUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<TenantModuleItemDto>> GetTenantModulesAsync(
        Guid tenantId,
        string? preferredLanguage,
        CancellationToken cancellationToken = default
    );

    Task UpdateTenantModulesAsync(
        Guid tenantId,
        IReadOnlyList<TenantModuleItemRequest> modules,
        Guid? actorUserId,
        CancellationToken cancellationToken = default
    );
}
