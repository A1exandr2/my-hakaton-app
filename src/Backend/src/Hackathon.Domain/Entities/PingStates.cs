namespace Hackathon.Domain.Entities;

public class PingStats
{
    public ulong TotalPings { get; set; }
    public double SuccessfulRate { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public DateTime? LastCheck { get; set; }

    public PingStats(ulong totalPings, double successfulRate, double avgResponseTimeMs, DateTime? lastCheck)
    {
        TotalPings = totalPings;
        SuccessfulRate = successfulRate;
        AvgResponseTimeMs = avgResponseTimeMs;
        LastCheck = lastCheck;
    }
}