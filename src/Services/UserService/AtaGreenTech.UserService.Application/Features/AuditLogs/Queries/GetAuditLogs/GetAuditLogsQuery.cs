using AtaGreenTech.UserService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Application.Features.AuditLogs.Queries.GetAuditLogs;

/// <param name="Action">Tam eylem filtresi (ör. <c>LOGIN_SUCCESS</c>).</param>
/// <param name="SearchTerm">E-posta veya details içinde arama.</param>
public sealed record GetAuditLogsQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    string? Action = null,
    string? SearchTerm = null
) : IQuery<PagedResult<AuditLogDto>>, IPagedRequest;
