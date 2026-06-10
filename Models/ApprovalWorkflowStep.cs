namespace SheetFlow.Models;

public class ApprovalWorkflowStep
{
    public long Id { get; set; }
    public long ApprovalWorkflowId { get; set; }
    public int StepOrder { get; set; }
    public string ApproverType { get; set; } = string.Empty;
    public string? ApproverTarget { get; set; }
    public DateTime CreatedAt { get; set; }

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
