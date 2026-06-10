namespace SheetFlow.Models;

public class FormRequest
{
    public long Id { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public long FormTemplateId { get; set; }
    public long ApplicantId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? ApplicantName { get; set; }
    public string? FormName { get; set; }
}
