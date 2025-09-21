using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Commands;

public record DeleteServerCommand(int Id) : IRequest<string>;