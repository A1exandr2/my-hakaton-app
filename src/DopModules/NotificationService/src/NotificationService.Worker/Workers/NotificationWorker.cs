using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.Contracts.Messages;
using NotificationService.Worker.Models;
using NotificationService.EmailService.Services;

namespace NotificationService.Worker.Workers;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IEmailService _emailService;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    public NotificationWorker(
        ILogger<NotificationWorker> logger,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        IEmailService emailService)
    {
        _logger = logger;
        _rabbitMqSettings = rabbitMqSettings?.Value ?? throw new ArgumentNullException(nameof(rabbitMqSettings));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

        // Настройки JSON
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Notification Worker...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await InitializeRabbitMQ(stoppingToken))
                {
                    _logger.LogError("Failed to initialize RabbitMQ connection. Retrying in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    continue;
                }

                await ProcessMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in worker execution. Restarting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Notification Worker stopped");
    }

    private async Task<bool> InitializeRabbitMQ(CancellationToken cancellationToken)
    {
        int maxRetries = 10;
        int retryCount = 0;

        while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Очищаем предыдущие соединения
                CleanupResources();

                var factory = new ConnectionFactory
                {
                    HostName = _rabbitMqSettings.HostName,
                    Port = _rabbitMqSettings.Port,
                    UserName = _rabbitMqSettings.UserName,
                    Password = _rabbitMqSettings.Password,
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _connection.ConnectionShutdown += OnConnectionShutdown;
                
                _channel = _connection.CreateModel();
                _channel.ModelShutdown += OnModelShutdown;

                // Объявляем exchange и queue
                _channel.ExchangeDeclare(
                    exchange: _rabbitMqSettings.ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                _channel.QueueDeclare(
                    queue: _rabbitMqSettings.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _channel.QueueBind(
                    queue: _rabbitMqSettings.QueueName,
                    exchange: _rabbitMqSettings.ExchangeName,
                    routingKey: _rabbitMqSettings.RoutingKey);

                // Настройка QoS
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation("✅ RabbitMQ connection established");
                return true;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Attempt {RetryCount}/{MaxRetries}", retryCount, maxRetries);
                
                if (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
                {
                    // Exponential backoff
                    var delay = TimeSpan.FromMilliseconds(3000 * Math.Pow(2, retryCount - 1));
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        _logger.LogError("❌ Could not connect to RabbitMQ after {MaxRetries} attempts", maxRetries);
        return false;
    }

    private async Task ProcessMessagesAsync(CancellationToken stoppingToken)
    {
        if (_channel == null || !_channel.IsOpen)
        {
            throw new InvalidOperationException("RabbitMQ channel is not available");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                await ProcessMessageAsync(ea, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in message processing");
                TryRejectMessage(ea, requeue: false);
            }
        };

        consumer.Shutdown += (model, ea) =>
        {
            _logger.LogWarning("Consumer shutdown: {Cause}", ea.Cause);
            return Task.CompletedTask;
        };

        _channel.BasicConsume(
            queue: _rabbitMqSettings.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Worker started listening for messages on queue: {QueueName}", _rabbitMqSettings.QueueName);

        // Ждем отмены или потери соединения
        var connectionLostSource = new TaskCompletionSource<bool>();
        
        void OnConnectionLost(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("Connection lost: {Cause}", e.Cause);
            connectionLostSource.TrySetResult(true);
        }

        if (_connection != null)
        {
            _connection.ConnectionShutdown += OnConnectionLost;
        }
        
        if (_channel != null)
        {
            _channel.ModelShutdown += OnConnectionLost;
        }

        try
        {
            await Task.WhenAny(
                Task.Delay(Timeout.Infinite, stoppingToken),
                connectionLostSource.Task);
        }
        finally
        {
            if (_connection != null)
            {
                _connection.ConnectionShutdown -= OnConnectionLost;
            }
            
            if (_channel != null)
            {
                _channel.ModelShutdown -= OnConnectionLost;
            }
            
            // Вручную освобождаем ресурсы TaskCompletionSource
            connectionLostSource.TrySetCanceled();
        }
    }

    // NotificationService.Worker/Workers/NotificationWorker.cs
    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        if (_channel == null || _connection == null || !_connection.IsOpen)
        {
            _logger.LogError("RabbitMQ channel is not available for message processing");
            return;
        }

        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        
        _logger.LogDebug("Received message: {Message}", message);

        try
        {
            var alert = JsonSerializer.Deserialize<AlertNotification>(message);
            if (alert == null)
            {
                _logger.LogWarning("Failed to deserialize AlertNotification");
                TryRejectMessage(ea, false);
                return;
            }

            _logger.LogInformation("Processing alert for server: {ServerHost}", alert.ServerHost);

            await _emailService.SendAlertNotificationAsync(alert, cancellationToken);
            
            _logger.LogInformation("✅ Alert notifications sent for server: {ServerHost}. Emails: {EmailCount}", 
                alert.ServerHost, alert.Emails.Count);

            if (_channel.IsOpen)
            {
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error");
            TryRejectMessage(ea, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing alert notification");
            TryRejectMessage(ea, false);
        }
    }

    private void TryRejectMessage(BasicDeliverEventArgs ea, bool requeue)
    {
        try
        {
            if (_channel?.IsOpen == true)
            {
                _channel.BasicReject(ea.DeliveryTag, requeue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject message");
        }
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs ea)
    {
        _logger.LogWarning("RabbitMQ connection shutdown: {Cause}", ea.Cause);
    }

    private void OnModelShutdown(object? sender, ShutdownEventArgs ea)
    {
        _logger.LogWarning("RabbitMQ channel shutdown: {Cause}", ea.Cause);
    }

    private void CleanupResources()
    {
        try
        {
            if (_channel != null)
            {
                _channel.ModelShutdown -= OnModelShutdown;
                _channel.Close();
                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while cleaning up RabbitMQ resources");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping worker...");
        
        CleanupResources();
        
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Worker stopped successfully");
    }
}