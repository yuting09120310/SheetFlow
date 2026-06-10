using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class FormRequestRepository : IFormRequestRepository
{
    private readonly DapperDbContext _db;

    public FormRequestRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<FormRequest?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT r.*, u.display_name AS ApplicantName, t.name AS FormName
                    FROM [form_requests] r
                    LEFT JOIN [users] u ON r.[applicant_id] = u.[id]
                    LEFT JOIN [form_templates] t ON r.[form_template_id] = t.[id]
                    WHERE r.[id] = @Id";
        return await conn.QueryFirstOrDefaultAsync<FormRequest>(sql, new { Id = id });
    }

    public async Task<IEnumerable<FormRequest>> GetByApplicantAsync(long applicantId, string? status = null)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT r.*, u.display_name AS ApplicantName, t.name AS FormName
                    FROM [form_requests] r
                    LEFT JOIN [users] u ON r.[applicant_id] = u.[id]
                    LEFT JOIN [form_templates] t ON r.[form_template_id] = t.[id]
                    WHERE r.[applicant_id] = @ApplicantId";
        if (!string.IsNullOrEmpty(status))
            sql += " AND r.[status] = @Status";
        sql += " ORDER BY r.[created_at] DESC";
        return await conn.QueryAsync<FormRequest>(sql, new { ApplicantId = applicantId, Status = status });
    }

    public async Task<IEnumerable<FormRequest>> GetPendingAsync()
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT r.*, u.display_name AS ApplicantName, t.name AS FormName
                    FROM [form_requests] r
                    LEFT JOIN [users] u ON r.[applicant_id] = u.[id]
                    LEFT JOIN [form_templates] t ON r.[form_template_id] = t.[id]
                    WHERE r.[status] IN ('Pending','Resubmitted')
                    ORDER BY r.[submitted_at] ASC";
        return await conn.QueryAsync<FormRequest>(sql);
    }

    public async Task<IEnumerable<FormRequest>> SearchAsync(long? templateId, long? applicantId, DateTime? startDate, DateTime? endDate, string? status)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT r.*, u.display_name AS ApplicantName, t.name AS FormName
                    FROM [form_requests] r
                    LEFT JOIN [users] u ON r.[applicant_id] = u.[id]
                    LEFT JOIN [form_templates] t ON r.[form_template_id] = t.[id]
                    WHERE 1=1";
        var parameters = new DynamicParameters();
        if (templateId.HasValue) { sql += " AND r.[form_template_id] = @TemplateId"; parameters.Add("TemplateId", templateId.Value); }
        if (applicantId.HasValue) { sql += " AND r.[applicant_id] = @ApplicantId"; parameters.Add("ApplicantId", applicantId.Value); }
        if (startDate.HasValue) { sql += " AND r.[created_at] >= @StartDate"; parameters.Add("StartDate", startDate.Value); }
        if (endDate.HasValue) { sql += " AND r.[created_at] <= @EndDate"; parameters.Add("EndDate", endDate.Value.AddDays(1)); }
        if (!string.IsNullOrEmpty(status)) { sql += " AND r.[status] = @Status"; parameters.Add("Status", status); }
        sql += " ORDER BY r.[created_at] DESC";
        return await conn.QueryAsync<FormRequest>(sql, parameters);
    }

    public async Task<long> CreateAsync(FormRequest request)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [form_requests] ([request_no],[form_template_id],[applicant_id],[status],[submitted_at],[approved_at],[rejected_at],[created_at],[updated_at])
                    VALUES (@RequestNo,@FormTemplateId,@ApplicantId,@Status,@SubmittedAt,@ApprovedAt,@RejectedAt,@CreatedAt,@UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, request);
    }

    public async Task UpdateStatusAsync(long id, string status)
    {
        using var conn = _db.CreateConnection();
        var now = DateTime.UtcNow;
        var sql = "UPDATE [form_requests] SET [status] = @Status, [updated_at] = @Now";
        if (status == "Approved") sql += ", [approved_at] = @Now";
        if (status == "Rejected") sql += ", [rejected_at] = @Now";
        sql += " WHERE [id] = @Id";
        await conn.ExecuteAsync(sql, new { Id = id, Status = status, Now = now });
    }

    public async Task UpdateAsync(FormRequest request)
    {
        using var conn = _db.CreateConnection();
        var sql = @"UPDATE [form_requests] SET [status]=@Status,[submitted_at]=@SubmittedAt,
                    [approved_at]=@ApprovedAt,[rejected_at]=@RejectedAt,[updated_at]=@UpdatedAt
                    WHERE [id]=@Id";
        await conn.ExecuteAsync(sql, request);
    }

    public async Task<IEnumerable<FormRequestValue>> GetValuesAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<FormRequestValue>(
            "SELECT * FROM [form_request_values] WHERE [form_request_id] = @Id ORDER BY [id]",
            new { Id = requestId });
    }

    public async Task<long> CreateValueAsync(FormRequestValue value)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [form_request_values] ([form_request_id],[field_id],[field_key],[field_name],[field_value],[created_at],[updated_at])
                    VALUES (@FormRequestId,@FieldId,@FieldKey,@FieldName,@FieldValue,@CreatedAt,@UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, value);
    }

    public async Task DeleteValuesAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM [form_request_values] WHERE [form_request_id] = @Id",
            new { Id = requestId });
    }
}
