using MediatR;
using Hackathon.Application.Exceptions;
using Hackathon.Domain.Entities;
using Hackathon.Domain.Repositories;
using Hackathon.Domain.Services;
using AutoMapper;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Commands;

public class UpdateServerCommandHandler : IRequestHandler<UpdateServerCommand, string>
{
    private readonly IServersRepository _serversRepository;
    private readonly IIntervalService _intervalService;
    private readonly IMapper _mapper;

    public UpdateServerCommandHandler(
        IServersRepository serversRepository,
        IIntervalService intervalService,
        IMapper mapper)
    {
        _serversRepository = serversRepository;
        _intervalService = intervalService;
        _mapper = mapper;
    }

    public async Task<string> Handle(UpdateServerCommand request, CancellationToken ct)
    {
        var server = await _serversRepository.GetByIdAsync(request.id, ct)
            ?? throw new NotFoundException(nameof(Server), request.id);

        var intervalMinutes = _intervalService.Parse(request.interval);

        server.IntervalMinutes = intervalMinutes;

        await _serversRepository.UpdateAsync(server, ct);

        return "Server updated successfully";
    }
}