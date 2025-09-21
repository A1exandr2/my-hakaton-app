using Hackathon.Domain.Entities;

namespace Hackathon.Domain.Repositories;

public interface IServersRepository
{
    Task<Server> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Server>> GetAllAsync(
        int limit,
        int offset,
        string? query,
        CancellationToken cancellationToken);
    Task AddAsync(Server server, CancellationToken cancellationToken);
    Task PutIntervalAsync(int id, int interval, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task UpdateAsync(Server server, CancellationToken cancellationToken);
    Task<IEnumerable<Server>> GetAllAsync(
        CancellationToken cancellationToken);
}