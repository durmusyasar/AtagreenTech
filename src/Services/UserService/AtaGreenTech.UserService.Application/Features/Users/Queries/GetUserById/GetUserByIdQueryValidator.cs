using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Users.Queries.GetUserById;

internal sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
