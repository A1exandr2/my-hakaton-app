// Hackathon.Application.Commands.Servers.DeleteServerCommandHandler

using MediatR;
using Hackathon.Domain.Repositories;
using AutoMapper;
using Hackathon.Application.DTOs;

namespace Hackathon.Application.Servers.Commands;

public class DeleteServerCommandHandler : IRequestHandler<DeleteServerCommand, string>
{
    private readonly IServersRepository _serversRepository;
    private readonly IPingLogsRepository _pingLogsRepository;
    private readonly IMapper _mapper;

    public DeleteServerCommandHandler(
        IServersRepository serversRepository,
        IPingLogsRepository pingLogsRepository,
        IMapper mapper)
    {
        _serversRepository = serversRepository;
        _pingLogsRepository = pingLogsRepository;
        _mapper = mapper;
    }

    public async Task<string> Handle(DeleteServerCommand request, CancellationToken ct)
    {
        await _pingLogsRepository.DeleteLogsByServerIdAsync((uint)request.Id, ct);

        await _serversRepository.DeleteAsync(request.Id, ct);

        return null!;
        
    }
}