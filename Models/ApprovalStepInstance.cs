namespace SheetFlow.Models;

public class ApprovalStepInstance
{
    public long Id { get; set; }
    public long FormRequestId { get; set; }
    public int StepOrder { get; set; }
    public string ApproverType { get; set; } = string.Empty;
    public string? ApproverTarget { get; set; }
    public long? AssignedUserId { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? AssignedUserName { get; set; }
    public string ApproverLabel
    {
        get
        {
            return ApproverType switch
            {
                "ApplicantDepartmentManager" => "申請人部門主管",
                "SpecificDepartmentManager" => $"{ApproverTarget}主管",
                "SpecificUser" => ApproverTarget ?? "",
                "Role" => ApproverTarget switch
                {
                    "Admin" => "系統管理員",
                    "Manager" => "主管",
                    _ => ApproverTarget ?? ""
                },
                _ => ApproverType
            };
        }
    }
}
