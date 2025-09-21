using MediatR;
using Hackathon.Domain.Entities;
using Hackathon.Domain.Repositories;
using Hackathon.Domain.Services;
using AutoMapper;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Commands;

public class CreateServerCommandHandler : IRequestHandler<CreateServerCommand, string>
{
    private readonly IServersRepository _serversRepository;
    private readonly IIntervalService _intervalService;
    private readonly IMapper _mapper;

    public CreateServerCommandHandler(
        IServersRepository serversRepository,
        IIntervalService intervalService,
        IMapper mapper)
    {
        _serversRepository = serversRepository;
        _intervalService = intervalService;
        _mapper = mapper;
    }

    public async Task<string> Handle(CreateServerCommand request, CancellationToken ct)
    {
        var intervalMinutes = _intervalService.Parse(request.Server.IntervalMinutes!);
        var server = new Server(
            request.Server.Host!,
            request.Server.Ip!,
            intervalMinutes
        );

        await _serversRepository.AddAsync(server, ct);

        return "Server added successfully";
    }
}