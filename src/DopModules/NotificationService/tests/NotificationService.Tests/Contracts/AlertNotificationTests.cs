using NotificationService.Contracts.Messages;

namespace NotificationService.Tests.Contracts;

public class AlertNotificationTests
{
    [Fact]
    public void AlertNotification_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var alert = new AlertNotification();

        // Assert
        Assert.Equal(0u, alert.ServerId);
        Assert.Equal(string.Empty, alert.ServerHost);
        Assert.False(alert.IsSuccess);
        Assert.Equal(string.Empty, alert.ErrorMessage);
        Assert.Equal(0, alert.StatusCode);
        Assert.Equal(string.Empty, alert.Protocol);
        Assert.NotNull(alert.Emails);
        Assert.NotNull(alert.TelegramUsernames);
        Assert.Empty(alert.Emails);
        Assert.Empty(alert.TelegramUsernames);
    }

    [Fact]
    public void AlertNotification_WithValues_SetsPropertiesCorrectly()
    {
        // Act
        var alert = new AlertNotification
        {
            ServerId = 123,
            ServerHost = "example.com",
            IsSuccess = true,
            ErrorMessage = "Test error",
            StatusCode = 404,
            Protocol = "HTTP",
            Timestamp = DateTime.UtcNow,
            Emails = new List<string> { "test1@example.com", "test2@example.com" },
            TelegramUsernames = new List<string> { "@user1", "@user2" }
        };

        // Assert
        Assert.Equal(123u, alert.ServerId);
        Assert.Equal("example.com", alert.ServerHost);
        Assert.True(alert.IsSuccess);
        Assert.Equal("Test error", alert.ErrorMessage);
        Assert.Equal(404, alert.StatusCode);
        Assert.Equal("HTTP", alert.Protocol);
        Assert.Equal(2, alert.Emails.Count);
        Assert.Equal(2, alert.TelegramUsernames.Count);
        Assert.Contains("test1@example.com", alert.Emails);
        Assert.Contains("@user1", alert.TelegramUsernames);
    }

    [Fact]
    public void GetTitle_WhenSuccess_ReturnsSuccessTitle()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerHost = "example.com",
            IsSuccess = true
        };

        // Act
        var title = alert.GetTitle();

        // Assert
        Assert.Equal("✅ Сервер example.com восстановлен", title);
    }

    [Fact]
    public void GetTitle_WhenFailure_ReturnsFailureTitle()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ServerHost = "example.com",
            IsSuccess = false
        };

        // Act
        var title = alert.GetTitle();

        // Assert
        Assert.Equal("🚨 Сервер example.com упал!", title);
    }

    [Fact]
    public void GetMessage_WhenSuccess_ReturnsSuccessMessage()
    {
        // Arrange
        var alert = new AlertNotification
        {
            Protocol = "HTTP",
            IsSuccess = true
        };

        // Act
        var message = alert.GetMessage();

        // Assert
        Assert.Equal("Сервер снова отвечает.\nПротокол: HTTP", message);
    }

    [Fact]
    public void GetMessage_WhenFailure_ReturnsFailureMessage()
    {
        // Arrange
        var alert = new AlertNotification
        {
            ErrorMessage = "Connection timeout",
            StatusCode = 500,
            Protocol = "HTTPS",
            IsSuccess = false
        };

        // Act
        var message = alert.GetMessage();

        // Assert
        Assert.Equal("Ошибка: Connection timeout\nКод: 500\nПротокол: HTTPS", message);
    }
}