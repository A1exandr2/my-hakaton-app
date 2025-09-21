namespace Hackathon.Domain.Entities;

public class PingLogResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public PingLogResult(bool success, string? errorMessage, int statusCode)
    {
        Success = success;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }
}