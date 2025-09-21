using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hackathon.Domain.Entities;

namespace Hackathon.Domain.Repositories;

public interface IPingLogsRepository
{
    Task InsertAsync(PingLog log, CancellationToken ct = default);
    Task InsertBatchAsync(IEnumerable<PingLog> logs, CancellationToken ct = default);
    Task<List<PingLog>> GetByServerIdAsync(
    uint serverId,
    DateTime from,
    DateTime to,
    int limit = 50,
    int offset = 0,
    CancellationToken ct = default);
    Task<PingStats> GetStatsAsync(
    uint serverId,
    CancellationToken ct = default);

    Task<double> GetAverageResponseTimeAsync(
        uint serverId,
        string period,
        CancellationToken ct = default);

    Task<DashboardStats> GetOverviewStatsAsync(CancellationToken ct = default);
    Task DeleteLogsByServerIdAsync(uint serverId, CancellationToken ct = default);

    Task<PingLogResult?> GetLastPingLogByServerIdAsync(
    uint serverId,
    CancellationToken ct = default);
}