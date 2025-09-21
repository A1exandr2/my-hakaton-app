namespace Hackathon.Infrastructure.Config;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "notifications_queue";
    public string ExchangeName { get; set; } = "notifications_exchange";
    public string RoutingKey { get; set; } = "notification.status_report";
}