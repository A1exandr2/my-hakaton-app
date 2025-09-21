namespace Hackathon.Api.Common.Models;

public class ErrorResponse : ApiResponse
{
    public Dictionary<string, string[]>? Errors { get; init; }
    public required string TraceId { get; init; }
    public required string Title { get; init; }
}