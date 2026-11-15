using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Application.Features.AuditLogs.Queries.GetAuditLogs;

internal sealed class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, BaseSearchModel.MaxPageSize);
        RuleFor(x => x.Action).MaximumLength(AuditLog.ActionMaxLength).When(x => x.Action is not null);
        RuleFor(x => x.SearchTerm).MaximumLength(200).When(x => x.SearchTerm is not null);
    }
}
