using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.AuditLogs.Queries.GetAuditLogs;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/audit-logs</c>: kimlik denetim günlüğü (yalnızca <c>audit.read</c> — Admin).
/// </summary>
public sealed class AuditLogEndpoints : IEndpointModule
{
    public string Name => "audit-logs";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            string? action = null,
            string? searchTerm = null
        ) => sender.Send(new GetAuditLogsQuery(pageNumber, pageSize, action, searchTerm), cancellationToken))
            .RequireAuthorization(AuthorizationPolicies.AuditRead);
    }
}
