using SheetFlow.Models;

namespace SheetFlow.ViewModels;

public class DynamicFormSubmitViewModel
{
    public long FormTemplateId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantDepartment { get; set; } = string.Empty;
    public List<DynamicFieldViewModel> Fields { get; set; } = new();
    public List<FormRequest> AvailablePrerequisiteRequests { get; set; } = new();
    public long? SelectedPrerequisiteRequestId { get; set; }
    public string? PrerequisiteTemplateName { get; set; }
}

public class DynamicFieldViewModel
{
    public long FieldId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string? OptionsJson { get; set; }
    public string? DefaultValue { get; set; }
    public string? Value { get; set; }
}
