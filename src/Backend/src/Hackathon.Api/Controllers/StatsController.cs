using Hackathon.Api.Common.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Hackathon.Api.Common.Models;
using Hackathon.Application.DTOs;
using Hackathon.Application.Queries.Stats;

namespace Hackathon.Api.Controllers;

[Route("api/stats")]
public class StatsController : BaseApiController
{
    public StatsController(ISender sender) : base(sender)
    {
    }

    [HttpGet("overview")]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<DashboardStatsDto>), Status200OK)]
    public async Task<ApiResponse> GetDashboardOverview(CancellationToken cancellationToken)
    {
        var data = await _sender.Send(new GetDashboardOverviewQuery(), cancellationToken);

        return new SuccessResponse<DashboardStatsDto>
        {
            StatusCode = Status200OK,
            Message = null,
            Data = data
        };
    }
}