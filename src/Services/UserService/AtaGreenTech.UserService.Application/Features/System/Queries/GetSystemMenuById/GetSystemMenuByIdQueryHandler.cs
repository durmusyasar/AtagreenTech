using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemMenuById;

internal sealed class GetSystemMenuByIdQueryHandler(ISystemNavigationAdmin admin)
    : IQueryHandler<GetSystemMenuByIdQuery, SystemMenuDetailDto>
{
    public async Task<SystemMenuDetailDto> Handle(
        GetSystemMenuByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await admin.GetMenuAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(MenuDefinition), request.Id);
    }
}
