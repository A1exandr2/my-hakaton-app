// NotificationService.Tests/Services/EmailServiceTests.cs
using Moq;
using NotificationService.Contracts.Messages;
using NotificationService.EmailService.Models;
using NotificationService.EmailService.Services;
using System.Net.Mail;

namespace NotificationService.Tests.Services;

public class EmailServiceTests : IDisposable
{
    private readonly Mock<ISmtpClient> _smtpClientMock;
    private readonly SmtpSettings _smtpSettings;
    private readonly EmailService.Services.EmailService _emailService;

    public EmailServiceTests()
    {
        _smtpClientMock = new Mock<ISmtpClient>();
        _smtpSettings = new SmtpSettings
        {
            Host = "smtp.test.com",
            Port = 587,
            Username = "test@test.com",
            Password = "test-password",
            EnableSsl = true
        };

        _emailService = new EmailService.Services.EmailService(_smtpSettings, _smtpClientMock.Object);
    }

    public void Dispose()
    {
        _emailService.Dispose();
    }

    [Fact]
    public void Constructor_WithSmtpSettings_SetsSettingsCorrectly()
    {
        // Assert
        Assert.NotNull(_emailService);
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WithEmails_CallsSmtpClientForEachEmail()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerHost = "example.com",
            IsSuccess = true,
            Emails = new List<string> { "test1@example.com", "test2@example.com", "test3@example.com" }
        };

        _smtpClientMock.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                      .Returns(Task.CompletedTask);

        // Act
        await _emailService.SendAlertNotificationAsync(alert);

        // Assert
        _smtpClientMock.Verify(x => x.SendMailAsync(It.IsAny<MailMessage>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WithNoEmails_DoesNotCallSmtpClient()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerHost = "example.com",
            IsSuccess = true,
            Emails = new List<string>()
        };

        // Act
        await _emailService.SendAlertNotificationAsync(alert);

        // Assert
        _smtpClientMock.Verify(x => x.SendMailAsync(It.IsAny<MailMessage>()), Times.Never);
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WithValidAlert_SendsCorrectEmail()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerId = 123,
            ServerHost = "api.example.com",
            IsSuccess = false,
            ErrorMessage = "Connection timeout",
            StatusCode = 503,
            Protocol = "HTTPS",
            Emails = new List<string> { "admin@example.com" }
        };

        MailMessage capturedMessage = null;
        _smtpClientMock.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                      .Callback<MailMessage>(msg => capturedMessage = msg)
                      .Returns(Task.CompletedTask);

        // Act
        await _emailService.SendAlertNotificationAsync(alert);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Single(capturedMessage.To);
        Assert.Equal("admin@example.com", capturedMessage.To[0].Address);
        Assert.Equal("test@test.com", capturedMessage.From.Address);
        Assert.Contains("🚨 Сервер api.example.com упал!", capturedMessage.Subject);
        Assert.Contains("Connection timeout", capturedMessage.Body);
        Assert.Contains("503", capturedMessage.Body);
        Assert.Contains("HTTPS", capturedMessage.Body);
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WhenSuccess_SendsSuccessEmail()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerHost = "db.example.com",
            IsSuccess = true,
            Protocol = "TCP",
            Emails = new List<string> { "dba@example.com" }
        };

        MailMessage capturedMessage = null;
        _smtpClientMock.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                      .Callback<MailMessage>(msg => capturedMessage = msg)
                      .Returns(Task.CompletedTask);

        // Act
        await _emailService.SendAlertNotificationAsync(alert);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Contains("✅ Сервер db.example.com восстановлен", capturedMessage.Subject);
        Assert.Contains("Сервер снова отвечает", capturedMessage.Body);
        Assert.Contains("TCP", capturedMessage.Body);
    }

    [Fact]
    public async Task SendAlertNotificationAsync_WhenSmtpFails_ThrowsException()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerHost = "example.com",
            IsSuccess = true,
            Emails = new List<string> { "test@example.com" }
        };

        _smtpClientMock.Setup(x => x.SendMailAsync(It.IsAny<MailMessage>()))
                      .ThrowsAsync(new InvalidOperationException("SMTP error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _emailService.SendAlertNotificationAsync(alert));
    }

    [Fact]
    public void GenerateAlertEmailBody_WithSuccessAlert_ReturnsSuccessHtml()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerId = 456,
            ServerHost = "app.example.com",
            IsSuccess = true,
            Protocol = "HTTP/2",
            Timestamp = new DateTime(2024, 1, 15, 10, 30, 0),
            Emails = new List<string> { "dev@example.com", "ops@example.com" }
        };

        // Act
        var result = GenerateAlertEmailBody(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("✅", result);
        Assert.Contains("Сервер восстановлен и работает нормально", result);
        Assert.Contains("app.example.com", result);
        Assert.Contains("HTTP/2", result);
        Assert.Contains("456", result);
        Assert.Contains("dev@example.com, ops@example.com", result);
        Assert.DoesNotContain("Ошибка:", result);
    }

    [Fact]
    public void GenerateAlertEmailBody_WithFailureAlert_ReturnsFailureHtml()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerId = 789,
            ServerHost = "cache.example.com",
            IsSuccess = false,
            ErrorMessage = "Memory limit exceeded",
            StatusCode = 500,
            Protocol = "Redis",
            Timestamp = new DateTime(2024, 1, 15, 11, 45, 0),
            Emails = new List<string> { "alert@example.com" }
        };

        // Act
        var result = GenerateAlertEmailBody(alert);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("🚨", result);
        Assert.Contains("Memory limit exceeded", result);
        Assert.Contains("500", result);
        Assert.Contains("Redis", result);
        Assert.Contains("789", result);
        Assert.Contains("alert@example.com", result);
        Assert.Contains("Ошибка:", result);
    }

    private static string GenerateAlertEmailBody(AlertNotification alert)
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
}