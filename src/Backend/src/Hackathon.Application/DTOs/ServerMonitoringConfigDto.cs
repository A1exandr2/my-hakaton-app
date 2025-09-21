namespace Hackathon.Application.DTOs;

public record ServerMonitoringConfigDto
{
    public int Id { get; init; }
    public string Host { get; init; } = string.Empty;
    public int IntervalMinutes { get; init; }
    public int Protocol { get; init; }
}