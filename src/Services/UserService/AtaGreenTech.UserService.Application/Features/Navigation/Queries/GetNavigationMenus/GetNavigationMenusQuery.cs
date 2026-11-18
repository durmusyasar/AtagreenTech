using AtaGreenTech.UserService.Application.Features.Navigation.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Navigation.Queries.GetNavigationMenus;

public sealed record GetNavigationMenusQuery : IQuery<NavigationMenusResponse>;
