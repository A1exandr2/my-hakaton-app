namespace NotificationService.Contracts.Messages;

public class AlertNotification
{
    public uint ServerId { get; init; }
    public string ServerHost { get; init; } = string.Empty;
    public bool IsSuccess { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public string Protocol { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public List<string> Emails { get; init; } = new();
    public List<string> TelegramUsernames { get; init; } = new();

    public string GetTitle() => IsSuccess
        ? $"✅ Сервер {ServerHost} восстановлен"
        : $"🚨 Сервер {ServerHost} упал!";

    public string GetMessage() => IsSuccess
        ? $"Сервер снова отвечает.\nПротокол: {Protocol}"
        : $"Ошибка: {ErrorMessage}\nКод: {StatusCode}\nПротокол: {Protocol}";
}