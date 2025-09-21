using Hackathon.Domain.Entities;

namespace Hackathon.Api.Common.Models;

public class SuccessResponse<T> : ApiResponse
{
    public required T Data { get; init; }
}