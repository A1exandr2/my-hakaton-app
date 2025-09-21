using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Commands;

public record UpdateServerCommand(int id, string interval) : IRequest<string>;