using SheetFlow.Models;

namespace SheetFlow.ViewModels;

public class DashboardViewModel
{
    public int MyPendingRequests { get; set; }
    public int MyTotalRequests { get; set; }
    public int PendingApprovals { get; set; }
    public List<FormRequest> RecentRequests { get; set; } = new();
}
