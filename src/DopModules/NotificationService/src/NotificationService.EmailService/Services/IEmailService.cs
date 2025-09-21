// NotificationService.EmailService/Services/IEmailService.cs
using NotificationService.Contracts.Messages;

namespace NotificationService.EmailService.Services;

public interface IEmailService
{
    Task SendAlertNotificationAsync(
        AlertNotification alert,
        CancellationToken cancellationToken = default
    );
}