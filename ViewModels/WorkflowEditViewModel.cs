using SheetFlow.Models;

namespace SheetFlow.ViewModels;

public class WorkflowEditViewModel
{
    public ApprovalWorkflow Workflow { get; set; } = new();
    public List<FormTemplate> Templates { get; set; } = new();
    public List<string> Departments { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<ApprovalWorkflow> ExistingWorkflows { get; set; } = new();
}
