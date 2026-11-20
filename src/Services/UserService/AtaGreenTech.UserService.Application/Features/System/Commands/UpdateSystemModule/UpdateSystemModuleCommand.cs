using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.UpdateSystemModule;

public sealed record UpdateSystemModuleCommand(
    Guid Id,
    string IconKey,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SystemMenuTranslationInput> Translations
) : ICommand;
