namespace SheetFlow.Models;

public class FormRequestValue
{
    public long Id { get; set; }
    public long FormRequestId { get; set; }
    public long FieldId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? FieldValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
