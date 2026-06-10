namespace SheetFlow.Models;

public class FormTemplateField
{
    public long Id { get; set; }
    public long FormTemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldType { get; set; } = "Text";
    public bool IsRequired { get; set; } = true;
    public int SortOrder { get; set; }
    public string? OptionsJson { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
