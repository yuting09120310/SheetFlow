namespace SheetFlow.Models;

public class FormRequestDependency
{
    public long Id { get; set; }
    public long FormRequestId { get; set; }
    public long RequiredRequestId { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? RequiredRequestNo { get; set; }
    public string? RequiredFormName { get; set; }
}
