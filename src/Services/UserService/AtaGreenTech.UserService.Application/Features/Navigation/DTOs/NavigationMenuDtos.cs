namespace AtaGreenTech.UserService.Application.Features.Navigation.DTOs;

public sealed record NavigationMenusResponse(IReadOnlyList<NavigationMenuGroupDto> Groups);

public sealed record NavigationMenuGroupDto(
    string ModuleCode,
    string ServiceName,
    string Title,
    IReadOnlyDictionary<string, string> Translations,
    IReadOnlyList<NavigationMenuItemDto> Items
);

public sealed record NavigationMenuItemDto(
    string Code,
    string Route,
    string Icon,
    string Title,
    IReadOnlyDictionary<string, string> Translations,
    IReadOnlyList<NavigationMenuItemDto>? Children = null,
    string? RequiredPermission = null
);
