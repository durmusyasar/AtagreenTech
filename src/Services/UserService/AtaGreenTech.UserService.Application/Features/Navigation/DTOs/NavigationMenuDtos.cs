namespace AtaGreenTech.UserService.Application.Features.Navigation.DTOs;

public sealed record NavigationMenusResponse(IReadOnlyList<NavigationMenuGroupDto> Groups);

public sealed record NavigationMenuGroupDto(
    string ServiceName,
    string TranslationKey,
    IReadOnlyList<NavigationMenuItemDto> Items
);

public sealed record NavigationMenuItemDto(
    string Code,
    string Route,
    string Icon,
    string TranslationKey,
    IReadOnlyList<NavigationMenuItemDto>? Children = null
);
