using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.PatchSystemMenu;

public sealed record PatchSystemMenuCommand(
    Guid Id,
    bool? IsActive = null,
    int? DisplayOrder = null
) : ICommand;
