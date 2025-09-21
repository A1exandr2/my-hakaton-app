using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Commands;

public record CreateServerCommand(ServerDto Server) : IRequest<string>;