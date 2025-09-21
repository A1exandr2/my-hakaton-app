using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Queries;

public record GetServerAvailabilityQuery(
    int ServerId,
    string Period = "24h") : IRequest<AvailabilityServerDto>;