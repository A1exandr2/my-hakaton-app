using Hackathon.Domain.Services;

namespace Hackathon.Application.Services;

public class IntervalService : IIntervalService
{
    public int Parse(string interval)
    {
        if (string.IsNullOrWhiteSpace(interval))
            throw new ArgumentException("Interval cannot be empty.");

        var match = System.Text.RegularExpressions.Regex.Match(interval, @"^(\d+)([hm])$");
        if (!match.Success)
            throw new ArgumentException($"Invalid interval format. Use '5m', '10h'.");

        var value = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value.ToLower();

        return unit switch
        {
            "m" => value,
            "h" => value * 60,
            _ => throw new ArgumentException($"Unsupported unit: {unit}")
        };
    }
}