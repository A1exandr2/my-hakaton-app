using MediatR;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Queries.Stats;

public record GetDashboardOverviewQuery : IRequest<DashboardStatsDto>;