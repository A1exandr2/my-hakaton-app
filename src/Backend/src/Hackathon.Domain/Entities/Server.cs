using System.Diagnostics.CodeAnalysis;
using Hackathon.Domain.Enums;

namespace Hackathon.Domain.Entities;

public class Server : BaseEntity
{
    public string? Host { get; set; }
    public string? Ip { get; set; }
    public int IntervalMinutes { get; set; }
    public Protocols Protocol { get; set; }

    protected Server() { }
    public Server(string host, string ip, int intervalMinutes, Protocols protocol = Protocols.ICMP)
    {
        Ip = ip;
        Host = host;
        IntervalMinutes = intervalMinutes;
        Protocol = protocol;
    }
}