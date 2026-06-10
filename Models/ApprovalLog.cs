namespace SheetFlow.Models;

public class ApprovalLog
{
    public long Id { get; set; }
    public long FormRequestId { get; set; }
    public string Action { get; set; } = string.Empty;
    public long ActorId { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? ActorName { get; set; }
}
