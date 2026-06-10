namespace SheetFlow.Models;

public class FormTemplate
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExcelFilePath { get; set; }
    public bool IsActive { get; set; } = true;
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
