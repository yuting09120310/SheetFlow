using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface INotificationLogRepository
{
    Task<long> CreateAsync(NotificationLog log);
}
