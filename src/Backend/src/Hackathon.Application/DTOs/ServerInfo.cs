using Hackathon.Domain.Enums;

namespace Hackathon.Application.DTOs;

public class ServerInfoDto
{
    public int Id { get; set; }
    public string? Ip { get; set; }
    public string? Host { get; set; }
    public string? Status { get; set; }
    public string? Protocol { get; set; }
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; }
    public ServerStatDto? Stats { get; set; }
}

public class ServerStatDto
{
    public ulong TotalPings { get; set; }
    public double SuccessRate { get; set; }
    public double AvgResponseTimeMs { get; set; }
    public DateTime? LastCheck { get; set; }
}