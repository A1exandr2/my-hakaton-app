using MediatR;
using Hackathon.Application.DTOs;
using Hackathon.Domain.Entities;
using Hackathon.Domain.Enums;
using Hackathon.Domain.Repositories;


namespace Hackathon.Application.Servers.Queries;

public class GetServerInfoAllQueryHandler : IRequestHandler<GetServerInfoAllQuery, List<ServerInfoDto>>
{
    private readonly IServersRepository _serversRepository;
    private readonly IPingLogsRepository _pingLogsRepository;

    public GetServerInfoAllQueryHandler(
        IServersRepository serversRepository,
        IPingLogsRepository pingLogsRepository)
    {
        _serversRepository = serversRepository;
        _pingLogsRepository = pingLogsRepository;
    }

    public async Task<List<ServerInfoDto>> Handle(GetServerInfoAllQuery request, CancellationToken ct)
    {
        var servers = await _serversRepository.GetAllAsync(request.Limit, request.Offset, request.Query, ct);

        var tasks = servers.Select(server => MapToServerInfoDtoAsync(server, ct));
        var serverInfos = await Task.WhenAll(tasks);

        return serverInfos.ToList();
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

