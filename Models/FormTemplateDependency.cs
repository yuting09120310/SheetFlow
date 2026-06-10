namespace SheetFlow.Models;

public class FormTemplateDependency
{
    public long Id { get; set; }
    public long FormTemplateId { get; set; }
    public long RequiredTemplateId { get; set; }
    public string RequiredStatus { get; set; } = "Approved";
    public DateTime CreatedAt { get; set; }

    public string? RequiredTemplateName { get; set; }
}
