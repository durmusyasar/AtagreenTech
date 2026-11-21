namespace AtaGreenTech.UserService.Application.Features.Tenants.DTOs;

/// <summary>Liste satırı için opsiyonel (non-core) modül rozeti.</summary>
/// <param name="Code">Modül kodu (örn. STOCK).</param>
/// <param name="IsExpired"><see langword="true"/> ise açık ama süresi dolmuş abonelik.</param>
public sealed record TenantModuleBadgeDto(string Code, bool IsExpired);

/// <summary>Platform tenant listesi satırı; modül özet sayaçları + rozetler.</summary>
/// <param name="ActiveModuleCount">Fiilen aktif tüm modüller (core + opsiyonel).</param>
/// <param name="OptionalActiveModuleCount">Fiilen aktif opsiyonel (!IsCore) modüller.</param>
/// <param name="ExpiredModuleCount">Açık ama süresi dolmuş opsiyonel abonelikler.</param>
/// <param name="ActiveModuleBadges">Opsiyonel modül rozetleri (aktif + süresi dolmuş uyarı).</param>
public sealed record TenantListItemDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    int ActiveModuleCount,
    int OptionalActiveModuleCount,
    int ExpiredModuleCount,
    IReadOnlyList<TenantModuleBadgeDto> ActiveModuleBadges,
    DateTime? CreatedAtUtc
);

public sealed record TenantListResponse(
    IReadOnlyList<TenantListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

/// <summary>Tenant modül abonelik satırı (çekmece).</summary>
/// <param name="IsEnabled">Ham abonelik / core kilidi (switch durumu).</param>
/// <param name="IsEffectivelyActive">
/// <see cref="TenantModuleSubscriptionRules.IsEffectivelyActive"/> — liste özeti ile tutarlı.
/// </param>
public sealed record TenantModuleItemDto(
    Guid ModuleId,
    string Code,
    string Name,
    IReadOnlyDictionary<string, string> Translations,
    bool IsCore,
    bool IsEnabled,
    DateTime? ExpiresAtUtc,
    bool IsEffectivelyActive
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
