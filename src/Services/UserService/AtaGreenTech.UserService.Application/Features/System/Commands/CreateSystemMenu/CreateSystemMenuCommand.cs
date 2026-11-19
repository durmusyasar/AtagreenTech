using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.CreateSystemMenu;

public sealed record CreateSystemMenuCommand(
    string Code,
    Guid ModuleId,
    Guid? ParentId,
    string RoutePath,
    string IconKey,
    string? RequiredPermission,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SystemMenuTranslationInput> Translations
) : ICommand<Guid>;
