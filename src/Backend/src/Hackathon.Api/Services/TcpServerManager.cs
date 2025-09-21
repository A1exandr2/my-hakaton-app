using Hackathon.Application.DTOs;
using Hackathon.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Hackathon.Api.Services;

public class TcpServerManager
{
    private readonly ILogger<TcpServerManager> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly List<TcpClient> _connectedClients = new();
    private readonly object _lock = new();

    public TcpServerManager(
        ILogger<TcpServerManager> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task SendServerListToAllClientsAsync(CancellationToken ct = default)
    {
        List<ServerMonitoringConfigDto> servers;
        using (var scope = _scopeFactory.CreateScope())
        {
            var serversRepository = scope.ServiceProvider.GetRequiredService<IServersRepository>();
            var allServers = await serversRepository.GetAllAsync(ct);

            servers = allServers.Select(s => new ServerMonitoringConfigDto
            {
                Id = s.Id,
                Host = s.Host!,
                IntervalMinutes = s.IntervalMinutes,
                Protocol = (int)s.Protocol
            }).ToList();
        }

        string json = JsonSerializer.Serialize(servers, new JsonSerializerOptions { WriteIndented = false });

        lock (_lock)
        {
            var deadClients = new List<TcpClient>();

            foreach (var client in _connectedClients)
            {
                try
                {
                    if (client.Connected)
                    {
                        var stream = client.GetStream();
                        var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                        writer.WriteLine(json);
                        _logger.LogInformation("📤 Отправлен список серверов клиенту {Client}", client.Client.RemoteEndPoint);
                    }
                    else
                    {
                        deadClients.Add(client);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Ошибка при отправке серверов клиенту");
                    deadClients.Add(client);
                }
            }

            foreach (var dead in deadClients)
            {
                _connectedClients.Remove(dead);
                dead.Close();
            }
        }
    }
    internal void RegisterClient(TcpClient client)
    {
        lock (_lock)
        {
            _connectedClients.Add(client);
            _logger.LogInformation("🔌 Новый клиент подключён: {Client}", client.Client.RemoteEndPoint);
        }
    }
    internal void UnregisterClient(TcpClient client)
    {
        lock (_lock)
        {
            _connectedClients.Remove(client);
            _logger.LogInformation("🔌 Клиент отключён: {Client}", client.Client.RemoteEndPoint);
        }
    }
}