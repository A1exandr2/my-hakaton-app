using Hackathon.Domain.Repositories;
using Hackathon.Domain.Entities;
using Hackathon.Infrastructure.Data;
using Hackathon.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Hackathon.Application.DTOs;
using Hackathon.Domain.Enums;

namespace Hackathon.Infrastructure.Repositories;

public class ServersRepository : IServersRepository
{
    private readonly AppDbContext _context;

    public ServersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Server> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        await _context.Servers
        .FirstOrDefaultAsync(w => w.Id.Equals(id)) ??
        throw new NotFoundException(nameof(User), id);


    public async Task<List<Server>> GetAllAsync(
        int limit,
        int offset,
        string? query,
        CancellationToken cancellationToken)
    {
        var serversQuery = _context.Servers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.ToLower();
            serversQuery = serversQuery.Where(w => w.Host!.ToLower().Contains(normalizedQuery));
        }

        return await serversQuery.OrderBy(w => w.Id).Skip(offset).Take(limit).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Server server, CancellationToken cancellationToken)
    {
        await _context.Servers.AddAsync(server, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task PutIntervalAsync(int id, int interval, CancellationToken cancellationToken)
    {
        var updatedServer = await _context.Servers.FindAsync(id);

        if (updatedServer is null)
            throw new NotFoundException(nameof(Server), id);

        updatedServer.IntervalMinutes = interval;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var updatedServer = await _context.Servers.FindAsync(id);

        if (updatedServer is null)
            throw new NotFoundException(nameof(Server), id);

        _context.Servers.Remove(updatedServer);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Server server, CancellationToken cancellationToken)
    {
        _context.Servers.Update(server);
        await _context.SaveChangesAsync(cancellationToken);
    }


    public async Task<IEnumerable<Server>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Servers.ToListAsync(cancellationToken);
    }
}