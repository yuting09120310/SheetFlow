using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface IApprovalWorkflowRepository
{
    Task<ApprovalWorkflow?> GetByIdAsync(long id);
    Task<IEnumerable<ApprovalWorkflow>> GetByTemplateIdAsync(long templateId);
    Task<ApprovalWorkflow?> GetByTemplateAndDepartmentAsync(long templateId, string? department);
    Task<IEnumerable<ApprovalWorkflowStep>> GetStepsAsync(long workflowId);
    Task<long> CreateAsync(ApprovalWorkflow workflow);
    Task UpdateAsync(ApprovalWorkflow workflow);
    Task DeleteAsync(long id);
    Task DeleteStepsAsync(long workflowId);
    Task<long> CreateStepAsync(ApprovalWorkflowStep step);
    Task ReorderStepsAsync(long workflowId, Dictionary<long, int> stepOrders);
    Task<IEnumerable<ApprovalStepInstance>> GetStepInstancesByRequestAsync(long requestId);
    Task<ApprovalStepInstance?> GetCurrentStepInstanceAsync(long requestId);
    Task<long> CreateStepInstanceAsync(ApprovalStepInstance instance);
    Task UpdateStepInstanceAsync(ApprovalStepInstance instance);
    Task<IEnumerable<ApprovalStepInstance>> GetPendingStepsByUserAsync(long userId);
    Task<IEnumerable<FormTemplateDependency>> GetDependenciesByTemplateAsync(long templateId);
    Task<long> CreateDependencyAsync(FormTemplateDependency dependency);
    Task DeleteDependencyAsync(long id);
    Task<long> CreateRequestDependencyAsync(FormRequestDependency dependency);
    Task<IEnumerable<FormRequestDependency>> GetDependenciesByRequestAsync(long requestId);
    Task<IEnumerable<FormRequest>> GetApprovedRequestsByUserAndTemplateAsync(long userId, long templateId);
}
