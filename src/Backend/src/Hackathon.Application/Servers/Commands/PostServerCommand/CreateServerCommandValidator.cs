using MediatR;
using Hackathon.Application.DTOs;
using FluentValidation;
using System.Data;

namespace Hackathon.Application.Servers.Commands;

public class PostServerCommandValidator : AbstractValidator<CreateServerCommand>
{
    public PostServerCommandValidator()
    {
        RuleFor(w => w.Server.Host)
            .NotEmpty().WithMessage("Host is required.")
            .MaximumLength(255).WithMessage("Host must not exceed 255 characters.");

        RuleFor(w => w.Server.Ip)
            .NotEmpty().WithMessage("IP address is required.")
            .MaximumLength(255).WithMessage("IP must not exceed 255 characters.")
            .Matches(@"^(?:\d{1,3}\.){3}\d{1,3}$")
            .WithMessage("Invalid IP address format.");

        RuleFor(w => w.Server.IntervalMinutes)
            .NotEmpty().WithMessage("Interval is required.");
    }
}