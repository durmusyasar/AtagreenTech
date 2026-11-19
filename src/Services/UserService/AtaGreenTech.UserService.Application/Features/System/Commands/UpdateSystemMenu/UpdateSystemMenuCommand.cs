using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.UpdateSystemMenu;

public sealed record UpdateSystemMenuCommand(
    Guid Id,
    string Code,
    Guid ModuleId,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SystemMenuTranslationInput> Translations
) : ICommand;
