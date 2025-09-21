using Hackathon.Application.DTOs;
using Hackathon.Infrastructure.Config;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Hackathon.Infrastructure.Services;

public class RabbitMqPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(RabbitMqSettings settings)
    {
        _settings = settings;

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: settings.ExchangeName,
            type: ExchangeType.Direct,
            durable: true);

        _channel.QueueDeclare(
            queue: settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: settings.QueueName,
            exchange: settings.ExchangeName,
            routingKey: settings.RoutingKey);
    }

    public void PublishAlert(AlertNotification alert)
    {
        var json = JsonSerializer.Serialize(alert);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(
            exchange: _settings.ExchangeName,
            routingKey: _settings.RoutingKey,
            basicProperties: null,
            body: body);

        Console.WriteLine($"📤 Отправлено в RabbitMQ: {alert.GetTitle()}");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
