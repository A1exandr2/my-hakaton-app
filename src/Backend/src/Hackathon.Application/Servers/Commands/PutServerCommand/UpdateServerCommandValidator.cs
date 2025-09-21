using MediatR;
using Hackathon.Application.DTOs;
using FluentValidation;
using System.Data;

namespace Hackathon.Application.Servers.Commands;

public class UpdateServerCommandValidator : AbstractValidator<UpdateServerCommand>
{
    public UpdateServerCommandValidator()
    {
        RuleFor(w => w.id).GreaterThan(0)
            .WithMessage("Server Id must be greater than 0.");;

        RuleFor(w => w.interval)
            .NotEmpty().WithMessage("Interval is required.");
    }
}