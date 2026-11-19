using AtaGreenTech.UserService.Application.Features.System.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemMenus;

public sealed record GetSystemMenusQuery : IQuery<IReadOnlyList<SystemMenuListItemDto>>;
