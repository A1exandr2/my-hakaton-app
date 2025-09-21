using Hackathon.Domain.Enums;

public class PingLog
{
    public DateTime Timestamp { get; private set; }
    public uint ServerId { get; private set; }
    public float ResponseTimeMs { get; private set; }
    public bool Success { get; private set; }
    public string ErrorMessage { get; private set; }
    public int StatusCode { get; private set; }
    public Protocols Protocol { get; private set; }

    public PingLog(
        DateTime timestamp,
        uint serverId,
        float responseTimeMs,
        bool success,
        string errorMessage,
        int statusCode = 0,
        Protocols protocol = Protocols.ICMP)
    {
        Timestamp = timestamp;
        ServerId = serverId;
        ResponseTimeMs = responseTimeMs;
        Success = success;
        ErrorMessage = errorMessage ?? string.Empty;

        StatusCode = statusCode;
        Protocol = protocol;
    }
}