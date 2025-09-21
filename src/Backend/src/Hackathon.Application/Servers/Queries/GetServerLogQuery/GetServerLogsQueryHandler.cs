using MediatR;
using Hackathon.Application.DTOs;
using Hackathon.Domain.Repositories;

namespace Hackathon.Application.Servers.Queries;

public class GetServerLogsQueryHandler : IRequestHandler<GetServerLogsQuery, List<PingLogDto>>
{
    private readonly IPingLogsRepository _pingLogsRepository;

    public GetServerLogsQueryHandler(IPingLogsRepository pingLogsRepository)
    {
        _pingLogsRepository = pingLogsRepository;
    }

    public async Task<List<PingLogDto>> Handle(GetServerLogsQuery request, CancellationToken ct)
    {
        var logs = await _pingLogsRepository.GetByServerIdAsync(
            (uint)request.ServerId,
            request.From ?? DateTime.MinValue,
            request.To ?? DateTime.MaxValue,
            request.Limit,
            request.Offset,
            ct);

        return logs.Select(log => new PingLogDto
        {
            Id = (int)log.ServerId,
            Timestamp = log.Timestamp,
            ResponseTimeMs = log.ResponseTimeMs,
            Success = log.Success,
            StatusCode = log.StatusCode,
            Protocol = log.Protocol,
            ErrorMessage = log.ErrorMessage ?? string.Empty
        }).ToList();
    }
}