using AtaGreenTech.UserService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Application.Features.Users.Queries.GetUsers;

/// <param name="SearchTerm">E-posta, kullanıcı adı veya ad/soyadda aranan metin.</param>
public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    string? SearchTerm = null,
    bool? IsActive = null
) : IQuery<PagedResult<UserDto>>, IPagedRequest;
