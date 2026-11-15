using FluentValidation;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Application.Features.Users.Queries.GetUsers;

internal sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, BaseSearchModel.MaxPageSize);
        RuleFor(x => x.SearchTerm).MaximumLength(200).When(x => x.SearchTerm is not null);
    }
}
