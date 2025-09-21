using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Queries;

public record GetServerLogsQuery(
    int ServerId,
    int Limit = 50,
    int Offset = 0,
    DateTime? From = null,
    DateTime? To = null) : IRequest<List<PingLogDto>>;