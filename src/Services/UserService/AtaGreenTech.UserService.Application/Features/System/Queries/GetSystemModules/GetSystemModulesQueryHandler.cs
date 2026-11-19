using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemModules;

internal sealed class GetSystemModulesQueryHandler(ISystemNavigationAdmin admin)
    : IQueryHandler<GetSystemModulesQuery, IReadOnlyList<SystemModuleDto>>
{
    public Task<IReadOnlyList<SystemModuleDto>> Handle(
        GetSystemModulesQuery request,
        CancellationToken cancellationToken
    ) => admin.GetModulesAsync(cancellationToken);
}
