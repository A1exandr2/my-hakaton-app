using MediatR;
using Hackathon.Application.DTOs;
using FluentValidation;

namespace Hackathon.Application.Servers.Commands;

public class DeleteServerCommandValidator : AbstractValidator<DeleteServerCommand>
{
    public DeleteServerCommandValidator()
    {
        RuleFor(w => w.Id).GreaterThan(0)
            .WithMessage("Server Id must be greater than 0.");;
    }
}