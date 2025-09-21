using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using Microsoft.Extensions.Configuration;
using Hackathon.Application.Exceptions;
using Hackathon.Domain.Entities;
using Hackathon.Domain.Repositories;
using Hackathon.Domain.Enums;
using ClickHouse.Client.Copy; // For ClickHouseBulkCopy

namespace Hackathon.Infrastructure.Repositories;

public class PingLogsClickHouseRepository : IPingLogsRepository
{
    private readonly string _connectionString;

    public PingLogsClickHouseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("ClickHouse") 
            ?? throw new InvalidOperationException("ClickHouse connection string is not configured.");
    }

    public async Task InsertAsync(PingLog log, CancellationToken ct = default)
    {
        const string query = @"
            INSERT INTO monitoring.ping_logs 
            (server_id, timestamp, response_time_ms, success, error_message, status_code, protocol)
            VALUES (@server_id, @timestamp, @response_time_ms, @success, @error_message, @status_code, @protocol)";

        await using var connection = await OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        AddParameter(cmd, "server_id", log.ServerId);
        AddParameter(cmd, "timestamp", log.Timestamp);
        AddParameter(cmd, "response_time_ms", log.ResponseTimeMs);
        AddParameter(cmd, "success", log.Success ? (byte)1 : (byte)0);
        AddParameter(cmd, "error_message", log.ErrorMessage ?? string.Empty);
        AddParameter(cmd, "status_code", log.StatusCode);
        AddParameter(cmd, "protocol", log.Protocol.ToString());

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task InsertBatchAsync(IEnumerable<PingLog> logs, CancellationToken ct = default)
    {
        foreach (var log in logs)
        {
            await InsertAsync(log, ct);
        }
    }

    public async Task<List<PingLog>> GetByServerIdAsync(
        uint serverId,
        DateTime from,
        DateTime to,
        int limit = 50,
        int offset = 0,
        CancellationToken ct = default)
    {
        const string query = @"
            SELECT 
                timestamp, 
                response_time_ms, 
                success, 
                error_message,
                status_code,
                protocol
            FROM monitoring.ping_logs
            WHERE server_id = @server_id 
            AND timestamp >= @from 
            AND timestamp <= @to
            ORDER BY timestamp DESC
            LIMIT @limit OFFSET @offset";

        await using var connection = await OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        AddParameter(cmd, "server_id", serverId);
        AddParameter(cmd, "from", from);
        AddParameter(cmd, "to", to);
        AddParameter(cmd, "limit", limit);
        AddParameter(cmd, "offset", offset);

        var logs = new List<PingLog>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            logs.Add(new PingLog(
                reader.GetDateTime(0),
                serverId,
                reader.GetFloat(1),
                reader.GetFieldValue<byte>(2) == 1,
                reader.GetString(3),
                reader.GetInt32(4),
                Enum.Parse<Protocols>(reader.GetString(5))
            ));
        }
        return logs;
    }

public async Task<PingLogResult?> GetLastPingLogByServerIdAsync(
    uint serverId,
    CancellationToken ct = default)
{
    const string query = @"
        SELECT success, error_message, status_code
        FROM monitoring.ping_logs
        WHERE server_id = @server_id
        ORDER BY timestamp DESC
        LIMIT 1";

    await using var connection = await OpenConnectionAsync(ct);
    await using var cmd = connection.CreateCommand();
    cmd.CommandText = query;

    AddParameter(cmd, "server_id", serverId);

    await using var reader = await cmd.ExecuteReaderAsync(ct);
    if (await reader.ReadAsync(ct))
    {
        return new PingLogResult(
            reader.GetFieldValue<byte>(0) == 1,
            reader.GetFieldValue<string>(1),
            reader.GetFieldValue<int>(2)
        );
    }

    return null;
}

    public async Task<PingStats> GetStatsAsync(
        uint serverId,
        CancellationToken ct = default)
    {
        const string query = @"
            SELECT 
                count() as total_pings,
                if(count() > 0, (sum(success) * 100.0) / count(), 0) as successful_rate,
                if(sum(success) > 0, avgIf(response_time_ms, success = 1), 0) as avg_response_time_ms,
                max(timestamp) as last_check
            FROM monitoring.ping_logs
            WHERE server_id = @server_id";

        await using var connection = await OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        AddParameter(cmd, "server_id", serverId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new PingStats(
                Convert.ToUInt64(reader.GetValue(0)),
                reader.GetFieldValue<double>(1),
                reader.GetFieldValue<double>(2),
                reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
            );
        }

        return new PingStats(0, 0.0, 0.0, null);
    }

    public async Task<double> GetAverageResponseTimeAsync(
        uint serverId,
        string period,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        DateTime from = period.ToLower() switch
        {
            "24h" => now.AddHours(-24),
            "7d" => now.AddDays(-7),
            "30d" => now.AddDays(-30),
            _ => throw new ArgumentException($"Unsupported period: {period}. Use '24h', '7d', or '30d'.", nameof(period))
        };

        const string query = @"
            SELECT 
                if(count() > 0, avgIf(response_time_ms, success = 1), 0) as avg_response_time_ms
            FROM monitoring.ping_logs
            WHERE server_id = @server_id 
            AND timestamp >= @from 
            AND timestamp <= @to";

        await using var connection = await OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        AddParameter(cmd, "server_id", serverId);
        AddParameter(cmd, "from", from);
        AddParameter(cmd, "to", now);

        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToDouble(result);
    }

    public async Task<DashboardStats> GetOverviewStatsAsync(CancellationToken ct = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        const string query = @"
            WITH 
                today_servers AS (
                    SELECT DISTINCT server_id
                    FROM monitoring.ping_logs
                    WHERE timestamp >= @todayStart AND timestamp < @tomorrowStart
                ),
                last_ping_per_server AS (
                    SELECT 
                        server_id,
                        argMax(success, timestamp) AS last_success
                    FROM monitoring.ping_logs
                    WHERE timestamp >= @todayStart AND timestamp < @tomorrowStart
                    GROUP BY server_id
                ),
                up_servers AS (
                    SELECT count() AS cnt
                    FROM last_ping_per_server
                    WHERE last_success = 1
                ),
                total_incidents AS (
                    SELECT count() AS cnt
                    FROM monitoring.ping_logs
                    WHERE success = 0 
                    AND timestamp >= @todayStart 
                    AND timestamp < @tomorrowStart
                )
            SELECT 
                (SELECT count() FROM today_servers) AS total_servers,
                (SELECT cnt FROM up_servers) AS up_servers,
                (SELECT count() FROM today_servers) - (SELECT cnt FROM up_servers) AS down_servers,
                (SELECT cnt FROM total_incidents) AS total_incidents_today";

        await using var connection = await OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        AddParameter(cmd, "todayStart", todayStart);
        AddParameter(cmd, "tomorrowStart", tomorrowStart);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            var totalServers = Convert.ToUInt64(reader.GetValue(0));
            var upServers = Convert.ToUInt64(reader.GetValue(1));
            var downServers = Convert.ToUInt64(reader.GetValue(2));
            var totalIncidentsToday = Convert.ToUInt64(reader.GetValue(3));

            return new DashboardStats(
                totalServers,
                upServers,
                downServers,
                totalIncidentsToday
            );
        }

        return new DashboardStats(0, 0, 0, 0);
    }

    public async Task DeleteLogsByServerIdAsync(uint serverId, CancellationToken ct = default)
    {
        const string query = @"
            ALTER TABLE monitoring.ping_logs 
            DELETE WHERE server_id = @server_id";

        await using var connection = await OpenConnectionAsync(ct);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        AddParameter(cmd, "server_id", serverId);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<ClickHouseConnection> OpenConnectionAsync(CancellationToken ct)
    {
        var connection = new ClickHouseConnection(_connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }

    private static void AddParameter(ClickHouseCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}