using Hackathon.Application.DTOs;
using Hackathon.Domain.Entities;
using Hackathon.Domain.Enums;
using Hackathon.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Hackathon.Infrastructure.Services;

namespace Hackathon.Api.Services;

public class TcpLogServer : BackgroundService
{
    private readonly ILogger<TcpLogServer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TcpServerManager _tcpServerManager;
    private readonly int _port = 7777;
    private readonly RabbitMqPublisher _rabbitMqPublisher;

    public TcpLogServer(
        ILogger<TcpLogServer> logger,
        IServiceScopeFactory scopeFactory,
        TcpServerManager tcpServerManager,
        RabbitMqPublisher rabbitMqPublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _tcpServerManager = tcpServerManager;
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        _logger.LogInformation("🚀 TCP-сервер запущен на порту {Port}", _port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                _ = HandleClientAsync(client, stoppingToken);
            }
        }
        finally
        {
            listener.Stop();
        }
    }

            private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
            {
                _tcpServerManager.RegisterClient(client);

                try
                {
                    using var stream = client.GetStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            _logger.LogInformation("📨 Получен JSON: {Line}", line);

                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };

                            var log = JsonSerializer.Deserialize<PingLogDto>(line, options);
                            if (log == null)
                            {
                                _logger.LogWarning("❌ Не удалось десериализовать JSON");
                                await writer.WriteLineAsync("ERROR: Invalid JSON format");
                                continue;
                            }

                            using var scope = _scopeFactory.CreateScope();
                            var pingLogsRepository = scope.ServiceProvider.GetRequiredService<IPingLogsRepository>();
                            var serversRepository = scope.ServiceProvider.GetRequiredService<IServersRepository>();
                            var usersRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                            var pingLog = new PingLog(
                                log.Timestamp,
                                (uint)log.Id,
                                (float)log.ResponseTimeMs,
                                log.Success,
                                log.ErrorMessage ?? string.Empty,
                                log.StatusCode,
                                MapProtocol((int)log.Protocol)
                            );

                            await pingLogsRepository.InsertAsync(pingLog, ct);
                            _logger.LogInformation("✅ Лог от сервера {ServerId} сохранён", log.Id);

                            var server = await serversRepository.GetByIdAsync((int)log.Id, ct);
                            if (server == null)
                            {
                                _logger.LogWarning("❌ Сервер с ID {ServerId} не найден в БД", log.Id);
                                await writer.WriteLineAsync("OK");
                                continue;
                            }

                            var users = await usersRepository.GetAllAsync(ct);

                            var emails = users
                                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                                .Select(u => u.Email)
                                .ToList();

                            var telegramUsernames = users
                                .Where(u => !string.IsNullOrWhiteSpace(u.Tg))
                                .Select(u => u.Tg)
                                .ToList();

                            if (emails.Count == 0 && telegramUsernames.Count == 0)
                            {
                                emails.Add("admin@example.com");
                                telegramUsernames.Add("@admin");
                            }

                            var alert = new AlertNotification
                            {
                                ServerId = (uint)log.Id,
                                ServerHost = server.Host!,
                                IsSuccess = log.Success,
                                ErrorMessage = log.ErrorMessage ?? "",
                                StatusCode = log.StatusCode,
                                Protocol = (int)log.Protocol switch
                                {
                                    1 => "HTTP",
                                    2 => "HTTPS",
                                    3 => "ICMP",
                                    _ => "UNKNOWN"
                                },
                                Timestamp = log.Timestamp,
                                Emails = emails,
                                TelegramUsernames = telegramUsernames!
                            };

                            _rabbitMqPublisher.PublishAlert(alert);

                            await writer.WriteLineAsync("OK");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Ошибка при обработке лога: {Line}", line);
                            await writer.WriteLineAsync($"ERROR: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Ошибка при обработке клиента");
                }
                finally
                {
                    _tcpServerManager.UnregisterClient(client);
                    client.Close();
                }
            }

    private static Protocols MapProtocol(int protocol)
    {
        return protocol switch
        {
            1 => Protocols.HTTP,
            2 => Protocols.HTTPS,
            3 => Protocols.ICMP,
            _ => Protocols.ICMP
        };
    }
}