using AtaGreenTech.UserService.Application.Features.Navigation.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Navigation.Queries.GetNavigationMenus;

/// <param name="PreferredLanguage">Accept-Language birincil etiket (ör. <c>tr</c>).</param>
public sealed record GetNavigationMenusQuery(string? PreferredLanguage = null)
    : IQuery<NavigationMenusResponse>;
