using MediatR;
using Hackathon.Application.DTOs;
using Hackathon.Application.Exceptions;
using Hackathon.Domain.Enums;
using Hackathon.Domain.Repositories;
using Hackathon.Domain.Entities;

namespace Hackathon.Application.Servers.Queries;

public class GetServerInfoByIdQueryHandler : IRequestHandler<GetServerInfoByIdQuery, ServerInfoDto>
{
    private readonly IServersRepository _serversRepository;
    private readonly IPingLogsRepository _pingLogsRepository;

    public GetServerInfoByIdQueryHandler(
        IServersRepository serversRepository,
        IPingLogsRepository pingLogsRepository)
    {
        _serversRepository = serversRepository;
        _pingLogsRepository = pingLogsRepository;
    }

    public async Task<ServerInfoDto> Handle(GetServerInfoByIdQuery request, CancellationToken ct)
    {
        var server = await _serversRepository.GetByIdAsync(request.Id, ct);
        if (server is null)
            throw new NotFoundException(nameof(Server), request.Id);

        return await MapToServerInfoDtoAsync(server, ct);
    }

    private async Task<ServerInfoDto> MapToServerInfoDtoAsync(Server server, CancellationToken ct)
    {
        var pingLogResult = await _pingLogsRepository.GetLastPingLogByServerIdAsync((uint)server.Id, ct);
        var statsData = await _pingLogsRepository.GetStatsAsync((uint)server.Id, ct);

        var status = pingLogResult?.Success == true
            ? ServerStatus.Up.ToString()
            : ServerStatus.Down.ToString();

        var statsDto = new ServerStatDto
        {
            TotalPings = statsData.TotalPings,
            SuccessRate = statsData.SuccessfulRate,
            AvgResponseTimeMs = statsData.AvgResponseTimeMs,
            LastCheck = statsData.LastCheck
        };

        var protocol = server.Protocol switch
        {
            Protocols.HTTP => Protocols.HTTP.ToString(),
            Protocols.HTTPS => Protocols.HTTPS.ToString(),
            Protocols.ICMP => Protocols.ICMP.ToString(),
            _ => Protocols.ICMP.ToString()
        };

        return new ServerInfoDto
        {
            Id = server.Id,
            Ip = server.Ip,
            Host = server.Host,
            Status = status,
            Stats = statsDto,
            ErrorMessage = pingLogResult?.ErrorMessage ?? string.Empty,
            StatusCode = pingLogResult?.StatusCode ?? 0,
            Protocol = protocol
        };
    }
}