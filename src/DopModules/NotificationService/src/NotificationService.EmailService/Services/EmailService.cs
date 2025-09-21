// NotificationService.EmailService/Services/EmailService.cs
using System.Net.Mail;
using NotificationService.Contracts.Messages;
using NotificationService.EmailService.Models;

namespace NotificationService.EmailService.Services;

public class EmailService : IEmailService, IDisposable
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ISmtpClient _smtpClient;

    public EmailService(SmtpSettings smtpSettings, ISmtpClient smtpClient)
    {
        _smtpSettings = smtpSettings;
        _smtpClient = smtpClient;
    }

    public async Task SendAlertNotificationAsync(AlertNotification alert, CancellationToken cancellationToken = default)
    {
        foreach (var email in alert.Emails)
        {
            await SendSingleAlertAsync(email, alert, cancellationToken);
        }
    }

    private async Task SendSingleAlertAsync(string email, AlertNotification alert, CancellationToken cancellationToken)
    {
        var subject = alert.GetTitle();
        var body = GenerateAlertEmailBody(alert);

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpSettings.Username),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(email);

        await _smtpClient.SendMailAsync(mailMessage);
    }

    private string GenerateAlertEmailBody(AlertNotification alert)
    {
        var statusIcon = alert.IsSuccess ? "✅" : "🚨";
        var statusColor = alert.IsSuccess ? "green" : "red";
        
        return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <h2 style='color: {statusColor};'>{statusIcon} {alert.GetTitle()}</h2>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                    <p><strong>Сервер:</strong> {alert.ServerHost}</p>
                    <p><strong>Протокол:</strong> {alert.Protocol}</p>
                    <p><strong>Время:</strong> {alert.Timestamp:dd.MM.yyyy HH:mm:ss}</p>
                    
                    {(alert.IsSuccess ? 
                        $@"<p style='color: green;'><strong>Статус:</strong> Сервер восстановлен и работает нормально</p>" :
                        $@"<p style='color: red;'><strong>Ошибка:</strong> {alert.ErrorMessage}</p>
                          <p><strong>Код ошибки:</strong> {alert.StatusCode}</p>")
                    }
                </div>

                <div style='margin-top: 20px; padding: 15px; background-color: #e9ecef; border-radius: 5px;'>
                    <p><strong>ID сервера:</strong> {alert.ServerId}</p>
                    <p><strong>Получатели:</strong> {string.Join(", ", alert.Emails)}</p>
                </div>

                <p style='color: #6c757d; font-size: 12px; margin-top: 20px;'>
                    Это автоматическое уведомление системы мониторинга. 
                    Пожалуйста, не отвечайте на это письмо.
                </p>
            </div>
        ";
    }

    public void Dispose()
    {
        (_smtpClient as IDisposable)?.Dispose();
    }
}