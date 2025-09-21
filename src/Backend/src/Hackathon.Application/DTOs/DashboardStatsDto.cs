namespace Hackathon.Application.DTOs;

public class DashboardStatsDto
{
    public ulong TotalServers { get; set; }
    public ulong UpServers { get; set; }
    public ulong DownServers { get; set; }
    public ulong TotalIncidentsToday { get; set; }
}