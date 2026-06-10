using SheetFlow.Models;

namespace SheetFlow.ViewModels;

public class RequestDetailViewModel
{
    public FormRequest Request { get; set; } = new();
    public List<FormRequestValue> Values { get; set; } = new();
    public List<ApprovalLog> ApprovalLogs { get; set; } = new();
    public List<ApprovalStepInstance> StepInstances { get; set; } = new();
    public List<FormTemplateField> Fields { get; set; } = new();
    public List<FormRequestDependency> Dependencies { get; set; } = new();
}
