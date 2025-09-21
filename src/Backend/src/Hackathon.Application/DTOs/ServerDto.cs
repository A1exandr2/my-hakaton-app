using System.Reflection.Metadata;

namespace Hackathon.Application.DTOs;

public class ServerDto
{
    public string? Host { get; set; }
    public string? Ip { get; set; }
    public string? IntervalMinutes { get; set; }
}