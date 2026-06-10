using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly DapperDbContext _db;

    public NotificationLogRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(NotificationLog log)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [notification_logs] ([form_request_id],[notify_type],[recipient],[subject],[content],[is_success],[error_message],[sent_at])
                    VALUES (@FormRequestId,@NotifyType,@Recipient,@Subject,@Content,@IsSuccess,@ErrorMessage,@SentAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, log);
    }
}
