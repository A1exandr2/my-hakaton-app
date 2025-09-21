using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Queries;

public record GetServerInfoByIdQuery(int Id) : IRequest<ServerInfoDto>;