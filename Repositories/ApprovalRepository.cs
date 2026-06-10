using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class ApprovalRepository : IApprovalRepository
{
    private readonly DapperDbContext _db;

    public ApprovalRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ApprovalLog>> GetByRequestIdAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT l.*, u.display_name AS ActorName
                    FROM [approval_logs] l
                    LEFT JOIN [users] u ON l.[actor_id] = u.[id]
                    WHERE l.[form_request_id] = @Id
                    ORDER BY l.[created_at] DESC";
        return await conn.QueryAsync<ApprovalLog>(sql, new { Id = requestId });
    }

    public async Task<long> CreateAsync(ApprovalLog log)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [approval_logs] ([form_request_id],[action],[actor_id],[comment],[created_at])
                    VALUES (@FormRequestId,@Action,@ActorId,@Comment,@CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, log);
    }
}
