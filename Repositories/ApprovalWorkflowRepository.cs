using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class ApprovalWorkflowRepository : IApprovalWorkflowRepository
{
    private readonly DapperDbContext _db;

    public ApprovalWorkflowRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<ApprovalWorkflow?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT w.*, t.name AS FormTemplateName
                    FROM [approval_workflows] w
                    LEFT JOIN [form_templates] t ON w.[form_template_id] = t.[id]
                    WHERE w.[id] = @Id";
        var workflow = await conn.QueryFirstOrDefaultAsync<ApprovalWorkflow>(sql, new { Id = id });
        if (workflow != null)
            workflow.Steps = (await GetStepsAsync(id)).ToList();
        return workflow;
    }

    public async Task<IEnumerable<ApprovalWorkflow>> GetByTemplateIdAsync(long templateId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT w.*, t.name AS FormTemplateName
                    FROM [approval_workflows] w
                    LEFT JOIN [form_templates] t ON w.[form_template_id] = t.[id]
                    WHERE w.[form_template_id] = @TemplateId AND w.[is_active] = 1
                    ORDER BY w.[department], w.[created_at]";
        var workflows = (await conn.QueryAsync<ApprovalWorkflow>(sql, new { TemplateId = templateId })).ToList();
        foreach (var w in workflows)
            w.Steps = (await GetStepsAsync(w.Id)).ToList();
        return workflows;
    }

    public async Task<ApprovalWorkflow?> GetByTemplateAndDepartmentAsync(long templateId, string? department)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT w.*, t.name AS FormTemplateName
                    FROM [approval_workflows] w
                    LEFT JOIN [form_templates] t ON w.[form_template_id] = t.[id]
                    WHERE w.[form_template_id] = @TemplateId AND w.[is_active] = 1
                    AND ((w.[department] = @Department) OR (w.[department] IS NULL AND @Department IS NULL))
                    ORDER BY CASE WHEN w.[department] IS NULL THEN 1 ELSE 0 END
                    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY";
        var workflow = await conn.QueryFirstOrDefaultAsync<ApprovalWorkflow>(sql, new { TemplateId = templateId, Department = (object?)department ?? DBNull.Value });
        if (workflow != null)
            workflow.Steps = (await GetStepsAsync(workflow.Id)).ToList();
        return workflow;
    }

    public async Task<IEnumerable<ApprovalWorkflowStep>> GetStepsAsync(long workflowId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<ApprovalWorkflowStep>(
            "SELECT * FROM [approval_workflow_steps] WHERE [approval_workflow_id] = @Id ORDER BY [step_order]",
            new { Id = workflowId });
    }

    public async Task<long> CreateAsync(ApprovalWorkflow workflow)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [approval_workflows] ([form_template_id],[department],[name],[is_active],[created_at],[updated_at])
                    VALUES (@FormTemplateId,@Department,@Name,@IsActive,GETUTCDATE(),GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, workflow);
    }

    public async Task UpdateAsync(ApprovalWorkflow workflow)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE [approval_workflows] SET [name]=@Name,[department]=@Department,[is_active]=@IsActive,[updated_at]=GETUTCDATE() WHERE [id]=@Id",
            workflow);
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM [approval_workflow_steps] WHERE [approval_workflow_id] = @Id", new { Id = id });
        await conn.ExecuteAsync("DELETE FROM [approval_workflows] WHERE [id] = @Id", new { Id = id });
    }

    public async Task DeleteStepsAsync(long workflowId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM [approval_workflow_steps] WHERE [approval_workflow_id] = @Id", new { Id = workflowId });
    }

    public async Task<long> CreateStepAsync(ApprovalWorkflowStep step)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [approval_workflow_steps] ([approval_workflow_id],[step_order],[approver_type],[approver_target],[created_at])
                    VALUES (@ApprovalWorkflowId,@StepOrder,@ApproverType,@ApproverTarget,GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, step);
    }

    public async Task ReorderStepsAsync(long workflowId, Dictionary<long, int> stepOrders)
    {
        using var conn = _db.CreateConnection();
        foreach (var (stepId, order) in stepOrders)
        {
            await conn.ExecuteAsync(
                "UPDATE [approval_workflow_steps] SET [step_order] = @Order WHERE [id] = @Id AND [approval_workflow_id] = @WorkflowId",
                new { Order = order, Id = stepId, WorkflowId = workflowId });
        }
    }

    public async Task<IEnumerable<ApprovalStepInstance>> GetStepInstancesByRequestAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT s.*, u.display_name AS AssignedUserName
                    FROM [approval_step_instances] s
                    LEFT JOIN [users] u ON s.[assigned_user_id] = u.[id]
                    WHERE s.[form_request_id] = @Id
                    ORDER BY s.[step_order]";
        return await conn.QueryAsync<ApprovalStepInstance>(sql, new { Id = requestId });
    }

    public async Task<ApprovalStepInstance?> GetCurrentStepInstanceAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT TOP 1 * FROM [approval_step_instances]
                    WHERE [form_request_id] = @Id AND [status] = 'Pending'
                    ORDER BY [step_order]";
        return await conn.QueryFirstOrDefaultAsync<ApprovalStepInstance>(sql, new { Id = requestId });
    }

    public async Task<long> CreateStepInstanceAsync(ApprovalStepInstance instance)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [approval_step_instances] ([form_request_id],[step_order],[approver_type],[approver_target],[assigned_user_id],[status],[created_at],[updated_at])
                    VALUES (@FormRequestId,@StepOrder,@ApproverType,@ApproverTarget,@AssignedUserId,@Status,GETUTCDATE(),GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, instance);
    }

    public async Task UpdateStepInstanceAsync(ApprovalStepInstance instance)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE [approval_step_instances] SET [status]=@Status,[assigned_user_id]=@AssignedUserId,[approved_at]=@ApprovedAt,[rejected_at]=@RejectedAt,[comment]=@Comment,[updated_at]=GETUTCDATE() WHERE [id]=@Id",
            instance);
    }

    public async Task<IEnumerable<ApprovalStepInstance>> GetPendingStepsByUserAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT s.*, u.display_name AS AssignedUserName
                    FROM [approval_step_instances] s
                    LEFT JOIN [users] u ON s.[assigned_user_id] = u.[id]
                    WHERE s.[assigned_user_id] = @UserId AND s.[status] = 'Pending'
                    ORDER BY s.[created_at]";
        return await conn.QueryAsync<ApprovalStepInstance>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<FormTemplateDependency>> GetDependenciesByTemplateAsync(long templateId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT d.*, t.name AS RequiredTemplateName
                    FROM [form_template_dependencies] d
                    LEFT JOIN [form_templates] t ON d.[required_template_id] = t.[id]
                    WHERE d.[form_template_id] = @TemplateId";
        return await conn.QueryAsync<FormTemplateDependency>(sql, new { TemplateId = templateId });
    }

    public async Task<long> CreateDependencyAsync(FormTemplateDependency dependency)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [form_template_dependencies] ([form_template_id],[required_template_id],[required_status],[created_at])
                    VALUES (@FormTemplateId,@RequiredTemplateId,@RequiredStatus,GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, dependency);
    }

    public async Task DeleteDependencyAsync(long id)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM [form_template_dependencies] WHERE [id] = @Id", new { Id = id });
    }

    public async Task<long> CreateRequestDependencyAsync(FormRequestDependency dependency)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [form_request_dependencies] ([form_request_id],[required_request_id],[created_at])
                    VALUES (@FormRequestId,@RequiredRequestId,GETUTCDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, dependency);
    }

    public async Task<IEnumerable<FormRequestDependency>> GetDependenciesByRequestAsync(long requestId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT d.*, r.request_no AS RequiredRequestNo, t.name AS RequiredFormName
                    FROM [form_request_dependencies] d
                    LEFT JOIN [form_requests] r ON d.[required_request_id] = r.[id]
                    LEFT JOIN [form_templates] t ON r.[form_template_id] = t.[id]
                    WHERE d.[form_request_id] = @Id";
        return await conn.QueryAsync<FormRequestDependency>(sql, new { Id = requestId });
    }

    public async Task<IEnumerable<FormRequest>> GetApprovedRequestsByUserAndTemplateAsync(long userId, long templateId)
    {
        using var conn = _db.CreateConnection();
        var sql = @"SELECT r.*, u.display_name AS ApplicantName, t.name AS FormName
                    FROM [form_requests] r
                    LEFT JOIN [users] u ON r.[applicant_id] = u.[id]
                    LEFT JOIN [form_templates] t ON r.[form_template_id] = t.[id]
                    WHERE r.[applicant_id] = @UserId AND r.[form_template_id] = @TemplateId AND r.[status] = 'Approved'
                    ORDER BY r.[approved_at] DESC";
        return await conn.QueryAsync<FormRequest>(sql, new { UserId = userId, TemplateId = templateId });
    }
}
