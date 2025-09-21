namespace Hackathon.Domain.Entities;
public class DashboardStats
{
    public ulong TotalServers { get; set; }
    public ulong UpServers { get; set; }
    public ulong DownServers { get; set; }
    public ulong TotalIncidentsToday { get; set; }

    public DashboardStats(
        ulong totalServers,
        ulong upServers,
        ulong downServers,
        ulong totalIncidentsToday)
    {
        TotalServers = totalServers;
        UpServers = upServers;
        DownServers = downServers;
        TotalIncidentsToday = totalIncidentsToday;
    }
}