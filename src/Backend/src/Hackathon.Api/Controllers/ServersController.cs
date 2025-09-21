using Hackathon.Api.Common.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.StatusCodes;
using Hackathon.Api.Common.Models;
using Hackathon.Application.DTOs;
using Hackathon.Application.Servers.Queries;
using Hackathon.Application.Servers.Commands;
using Hackathon.Api.Services;


namespace Hackathon.Api.Controllers;

[Route("api/servers")]
public class ServerController : BaseApiController
{
    private readonly TcpServerManager _tcpServerManager;
    public ServerController(TcpServerManager tcpServerManager, ISender sender) : base(sender)
    {
        _tcpServerManager = tcpServerManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<List<ServerInfoDto>>), Status200OK)]
    public async Task<ApiResponse> GetServerInfoAll([FromQuery] string? query, CancellationToken cancellationToken,
    [FromQuery] int limit = default, [FromQuery] int offset = default)
    {
        var data = await _sender.Send(new GetServerInfoAllQuery(query, offset, limit), cancellationToken);

        return new SuccessResponse<List<ServerInfoDto>>
        {
            StatusCode = Status200OK,
            Message = null,
            Data = data
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<ServerInfoDto>), Status200OK)]
    public async Task<ApiResponse> GetServerInfoById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var data = await _sender.Send(new GetServerInfoByIdQuery(id), cancellationToken);

        return new SuccessResponse<ServerInfoDto>
        {
            StatusCode = Status200OK,
            Message = null,
            Data = data
        };
    }

    [HttpGet("{id:int}/logs")]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<List<PingLogDto>>), Status200OK)]
    public async Task<ApiResponse> GetServerLogsById([FromRoute] int id, CancellationToken cancellationToken,
    [FromQuery] int limit = default,
    [FromQuery] int offset = default,
    [FromQuery] DateTime from = default,
    [FromQuery] DateTime to = default)
    {
        var data = await _sender.Send(new GetServerLogsQuery(id, limit, offset, from, to), cancellationToken);

        return new SuccessResponse<List<PingLogDto>>
        {
            StatusCode = Status200OK,
            Message = null,
            Data = data
        };
    }

    [HttpGet("{id:int}/stats")]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<AvailabilityServerDto>), Status200OK)]
    public async Task<ApiResponse> GetServerLogsById([FromRoute] int id, CancellationToken cancellationToken, [FromQuery] string period = default!)
    {
        var data = await _sender.Send(new GetServerAvailabilityQuery(id, period), cancellationToken);

        return new SuccessResponse<AvailabilityServerDto>
        {
            StatusCode = Status200OK,
            Message = null,
            Data = data
        };
    }

    [HttpPost]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<string>), Status200OK)]
    public async Task<ApiResponse> PostServer([FromBody] ServerDto server, CancellationToken cancellationToken)
    {
        var data = await _sender.Send(new CreateServerCommand(server), cancellationToken);

        await _tcpServerManager.SendServerListToAllClientsAsync(HttpContext.RequestAborted);
        return new SuccessResponse<ServerDto>
        {
            StatusCode = Status200OK,
            Message = data,
            Data = null!
        };
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<string>), Status200OK)]
    public async Task<ApiResponse> PutServer([FromRoute] int id, [FromBody] string interval, CancellationToken cancellationToken)
    {
        var data = await _sender.Send(new UpdateServerCommand(id, interval), cancellationToken);

        await _tcpServerManager.SendServerListToAllClientsAsync(HttpContext.RequestAborted);
        return new SuccessResponse<ServerDto>
        {
            StatusCode = Status200OK,
            Message = data,
            Data = null!
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ErrorResponse), Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorResponse), Status400BadRequest)]
    [ProducesResponseType(typeof(SuccessResponse<string>), Status200OK)]
    public async Task<ApiResponse> DeleteServer([FromRoute] int id, CancellationToken cancellationToken)
    {
        var data = await _sender.Send(new DeleteServerCommand(id), cancellationToken);

        await _tcpServerManager.SendServerListToAllClientsAsync(HttpContext.RequestAborted);
        return new SuccessResponse<ServerDto>
        {
            StatusCode = Status200OK,
            Message = data,
            Data = null!
        };
    }
}