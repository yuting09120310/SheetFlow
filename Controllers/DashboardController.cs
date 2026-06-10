using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Repositories;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IFormRequestRepository _requestRepo;
    private readonly IApprovalWorkflowRepository _workflowRepo;

    public DashboardController(IFormRequestRepository requestRepo, IApprovalWorkflowRepository workflowRepo)
    {
        _requestRepo = requestRepo;
        _workflowRepo = workflowRepo;
    }

    public async Task<IActionResult> Index()
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

        var myRequests = (await _requestRepo.GetByApplicantAsync(userId)).ToList();
        var recentRequests = myRequests.Take(10).ToList();

        var vm = new DashboardViewModel
        {
            MyPendingRequests = myRequests.Count(r => r.Status == "Rejected" || r.Status == "Resubmitted"),
            MyTotalRequests = myRequests.Count,
            RecentRequests = recentRequests
        };

        if (role == "Manager" || role == "Admin")
        {
            var pendingSteps = await _workflowRepo.GetPendingStepsByUserAsync(userId);
            vm.PendingApprovals = pendingSteps.Count();
        }

        return View(vm);
    }
}
