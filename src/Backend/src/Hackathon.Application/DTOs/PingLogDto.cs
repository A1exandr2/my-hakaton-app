using Hackathon.Domain.Enums;

namespace Hackathon.Application.DTOs;

public class PingLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double ResponseTimeMs { get; set; }
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public Protocols Protocol { get; set; }
    public string? ErrorMessage { get; set; }
}