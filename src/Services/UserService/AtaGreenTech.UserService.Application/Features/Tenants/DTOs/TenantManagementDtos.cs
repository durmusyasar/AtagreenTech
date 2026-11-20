namespace AtaGreenTech.UserService.Application.Features.Tenants.DTOs;

public sealed record TenantListItemDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    int EnabledModuleCount,
    DateTime? CreatedAtUtc
);

public sealed record TenantListResponse(
    IReadOnlyList<TenantListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public sealed record TenantModuleItemDto(
    Guid ModuleId,
    string Code,
    string Name,
    IReadOnlyDictionary<string, string> Translations,
    bool IsCore,
    bool IsEnabled,
    DateTime? ExpiresAtUtc
);

public sealed record TenantModuleItemRequest(
    Guid ModuleId,
    bool IsEnabled,
    DateTime? ExpiresAtUtc
);

public sealed record CreateTenantRequest(
    string Name,
    string Slug,
    IReadOnlyList<TenantModuleItemRequest>? Modules = null,
    string? AdminEmail = null,
    string? AdminFirstName = null,
    string? AdminLastName = null,
    string? AdminPassword = null
);

public sealed record CreateTenantResult(
    Guid Id,
    string Name,
    string Slug,
    bool IdentitySeeded,
    string? Message = null
);
