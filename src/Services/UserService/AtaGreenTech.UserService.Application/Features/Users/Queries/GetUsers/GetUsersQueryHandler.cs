using AtaGreenTech.UserService.Application.Common;
using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.UserService.Application.Features.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(IUserDbContext db)
    : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = db.Users;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLowerInvariant();
            users = users.Where(u =>
                u.Email.Contains(term)
                || u.Username.ToLower().Contains(term)
                || u.FirstName.ToLower().Contains(term)
                || u.LastName.ToLower().Contains(term)
            );
        }

        if (request.IsActive is { } isActive)
            users = users.Where(u => u.IsActive == isActive);

        return UserManagementHelpers
            .ProjectToDto(users.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
