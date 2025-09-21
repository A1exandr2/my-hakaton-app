using MediatR;
using Hackathon.Application.DTOs;
using Hackathon.Domain.Repositories;

namespace Hackathon.Application.Queries.Stats;

public class GetDashboardOverviewQueryHandler : IRequestHandler<GetDashboardOverviewQuery, DashboardStatsDto>
{
    private readonly IPingLogsRepository _pingLogsRepository;
    private readonly IServersRepository _serversRepository;

    public GetDashboardOverviewQueryHandler(
        IPingLogsRepository pingLogsRepository,
        IServersRepository serversRepository)
    {
        _pingLogsRepository = pingLogsRepository;
        _serversRepository = serversRepository;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardOverviewQuery request, CancellationToken ct)
    {
        var statServers = await _pingLogsRepository.GetOverviewStatsAsync(ct);

        return new DashboardStatsDto
        {
            TotalServers = statServers.TotalServers,
            UpServers = statServers.UpServers,
            DownServers = statServers.DownServers,
            TotalIncidentsToday = statServers.TotalIncidentsToday
        };
    }
}