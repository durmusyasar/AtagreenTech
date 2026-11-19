using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemLanguages;

internal sealed class GetSystemLanguagesQueryHandler(ISystemNavigationAdmin admin)
    : IQueryHandler<GetSystemLanguagesQuery, IReadOnlyList<SystemLanguageDto>>
{
    public Task<IReadOnlyList<SystemLanguageDto>> Handle(
        GetSystemLanguagesQuery request,
        CancellationToken cancellationToken
    ) => admin.GetLanguagesAsync(request.ActiveOnly, cancellationToken);
}
