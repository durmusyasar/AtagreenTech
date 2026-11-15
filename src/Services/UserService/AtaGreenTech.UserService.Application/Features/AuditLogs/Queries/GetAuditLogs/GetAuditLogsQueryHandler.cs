using AtaGreenTech.UserService.Application.Common;
using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.UserService.Application.Features.AuditLogs.Queries.GetAuditLogs;

internal sealed class GetAuditLogsQueryHandler(IUserDbContext db)
    : IQueryHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = db.AuditLogs;

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = AuditLog.NormalizeAction(request.Action);
            logs = logs.Where(l => l.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLowerInvariant();
            logs = logs.Where(l =>
                l.Email.Contains(term)
                || (l.Details != null && l.Details.ToLower().Contains(term))
            );
        }

        return logs
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Select(l => new AuditLogDto(
                l.Id,
                l.UserId,
                l.Email,
                l.Action,
                l.IpAddress,
                l.Details,
                l.CreatedAt
            ))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
