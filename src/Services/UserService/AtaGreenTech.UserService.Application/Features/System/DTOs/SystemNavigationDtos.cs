namespace AtaGreenTech.UserService.Application.Features.System.DTOs;

public sealed record SystemLanguageDto(
    Guid Id,
    string Code,
    string Name,
    bool IsDefault,
    bool IsActive
);

public sealed record SystemModuleDto(
    Guid Id,
    string Code,
    string ServiceName,
    string IconKey,
    int DisplayOrder,
    bool IsCore,
    bool IsActive,
    IReadOnlyDictionary<string, string> Translations
);

public sealed record SystemMenuListItemDto(
    Guid Id,
    string Code,
    Guid ModuleId,
    string ModuleCode,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyDictionary<string, string> Translations
);

public sealed record SystemMenuDetailDto(
    Guid Id,
    string Code,
    Guid ModuleId,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyDictionary<string, string> Translations
);

public sealed record SystemMenuTranslationInput(string LanguageCode, string Name);

public sealed record CreateSystemMenuRequest(
    string Code,
    Guid ModuleId,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SystemMenuTranslationInput> Translations
);

public sealed record UpdateSystemMenuRequest(
    string Code,
    Guid ModuleId,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SystemMenuTranslationInput> Translations
);

public sealed record PatchSystemMenuRequest(
    bool? IsActive = null,
    int? DisplayOrder = null
);

public sealed record UpdateSystemModuleRequest(
    string IconKey,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SystemMenuTranslationInput> Translations
);
