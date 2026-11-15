using AtaGreenTech.UserService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Queries.GetSessions;

/// <param name="CurrentRefreshToken">İsteğe bağlı; hash ile eşleşen oturum <c>IsCurrent</c> işaretlenir.</param>
public sealed record GetSessionsQuery(string? CurrentRefreshToken = null)
    : IQuery<IReadOnlyList<SessionDto>>;
