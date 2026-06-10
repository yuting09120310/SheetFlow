namespace SheetFlow.Models;

public class NotificationLog
{
    public long Id { get; set; }
    public long FormRequestId { get; set; }
    public string NotifyType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
}
