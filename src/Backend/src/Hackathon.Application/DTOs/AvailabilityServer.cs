namespace   Hackathon.Application.DTOs;

public class AvailabilityServerDto
{
    public int Id { get; set; }
    public string? Host { get; set; }
    public double avgResponseTimeMs {get; set; }
}