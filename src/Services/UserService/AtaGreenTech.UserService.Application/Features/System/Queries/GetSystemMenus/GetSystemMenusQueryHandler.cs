using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemMenus;

internal sealed class GetSystemMenusQueryHandler(ISystemNavigationAdmin admin)
    : IQueryHandler<GetSystemMenusQuery, IReadOnlyList<SystemMenuListItemDto>>
{
    public Task<IReadOnlyList<SystemMenuListItemDto>> Handle(
        GetSystemMenusQuery request,
        CancellationToken cancellationToken
    ) => admin.GetMenusAsync(cancellationToken);
}
