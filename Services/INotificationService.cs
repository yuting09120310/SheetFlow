namespace SheetFlow.Services;

public interface INotificationService
{
    Task NotifyManagersAsync(string subject, string message);
    Task NotifyUserAsync(long userId, string subject, string message);
}
