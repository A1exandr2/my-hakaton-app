using FluentValidation;

namespace Hackathon.Application.Servers.Queries;

public class GetServerAvailabilityQueryValidator : AbstractValidator<GetServerAvailabilityQuery>
{
    public GetServerAvailabilityQueryValidator()
    {
        RuleFor(w => w.ServerId).GreaterThan(0)
            .WithMessage("Server Id must be greater than 0.");
    }
}