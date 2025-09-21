using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Queries;

public record GetServerInfoAllQuery(
    string? Query,
    int Offset = 0,
    int Limit = 25) : IRequest<List<ServerInfoDto>>;