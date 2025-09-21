using MediatR;
using Hackathon.Application.DTOs;
using Hackathon.Domain.Repositories;
using Hackathon.Application.Exceptions;

namespace Hackathon.Application.Servers.Queries;

public class GetServerAvailabilityQueryHandler : IRequestHandler<GetServerAvailabilityQuery, AvailabilityServerDto>
{
    private readonly IPingLogsRepository _pingLogsRepository;
    private readonly IServersRepository _serversRepository;

    public GetServerAvailabilityQueryHandler(
        IPingLogsRepository pingLogsRepository,
        IServersRepository serversRepository)
    {
        _pingLogsRepository = pingLogsRepository;
        _serversRepository = serversRepository;
    }

    public async Task<AvailabilityServerDto> Handle(GetServerAvailabilityQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        DateTime from;

        switch (request.Period.ToLower())
        {
            case "24h":
                from = now.AddHours(-24);
                break;
            case "7d":
                from = now.AddDays(-7);
                break;
            case "30d":
                from = now.AddDays(-30);
                break;
            default:
                throw new ArgumentException("Invalid period. Use '24h', '7d', or '30d'.", nameof(request.Period));
        }

        var statsData = await _pingLogsRepository.GetAverageResponseTimeAsync(
            (uint)request.ServerId,
            request.Period,
            ct);

        var server = await _serversRepository.GetByIdAsync(request.ServerId, ct)
            ?? throw new NotFoundException(nameof(Servers), request.ServerId);

        return new AvailabilityServerDto
        {
            Id = request.ServerId,
            Host = server.Host,
            avgResponseTimeMs = statsData
        };
    }
}