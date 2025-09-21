using FluentValidation;

namespace Hackathon.Application.Servers.Queries;

public class GetServerLogsQueryValidator : AbstractValidator<GetServerLogsQuery>
{
    public GetServerLogsQueryValidator()
    {
        RuleFor(w => w.ServerId).GreaterThan(0)
            .WithMessage("Server Id must be greater than 0.");
    }
}