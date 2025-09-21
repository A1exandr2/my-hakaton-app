using FluentValidation;

namespace Hackathon.Application.Servers.Queries;

public class GetServerInfoByIdQueryValidator : AbstractValidator<GetServerInfoByIdQuery>
{
    public GetServerInfoByIdQueryValidator()
    {
        RuleFor(w => w.Id).GreaterThan(0)
            .WithMessage("Server Id must be greater than 0.");
    }
}