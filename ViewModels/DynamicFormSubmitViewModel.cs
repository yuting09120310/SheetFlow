using System.ComponentModel.DataAnnotations;

namespace SheetFlow.ViewModels;

public class DynamicFormSubmitViewModel
{
    public long FormTemplateId { get; set; }
    public string FormName { get; set; } = string.Empty;
    public List<DynamicFieldViewModel> Fields { get; set; } = new();
}

public class DynamicFieldViewModel
{
    public long FieldId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string? OptionsJson { get; set; }
    public string? DefaultValue { get; set; }

    [Display(Name = "值")]
    public string? Value { get; set; }
}
