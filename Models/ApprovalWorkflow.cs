namespace SheetFlow.Models;

public class ApprovalWorkflow
{
    public long Id { get; set; }
    public long FormTemplateId { get; set; }
    public string? Department { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? FormTemplateName { get; set; }
    public List<ApprovalWorkflowStep> Steps { get; set; } = new();
}
