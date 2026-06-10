using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Repositories;
using SheetFlow.Services;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ApprovalsController : Controller
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly IFormRequestRepository _requestRepo;
    private readonly IFormRequestService _requestService;

    public ApprovalsController(
        IApprovalWorkflowRepository workflowRepo,
        IFormRequestRepository requestRepo,
        IFormRequestService requestService)
    {
        _workflowRepo = workflowRepo;
        _requestRepo = requestRepo;
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var pendingSteps = await _workflowRepo.GetPendingStepsByUserAsync(userId);
        var stepList = pendingSteps.ToList();

        var requestIds = stepList.Select(s => s.FormRequestId).Distinct();
        var requests = new List<SheetFlow.Models.FormRequest>();
        foreach (var rid in requestIds)
        {
            var req = await _requestRepo.GetByIdAsync(rid);
            if (req != null) requests.Add(req);
        }

        ViewBag.StepMap = stepList.ToDictionary(s => s.FormRequestId, s => s);
        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(long id, string? comment)
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        try
        {
            await _requestService.ApproveRequestAsync(id, userId, comment);
            TempData["Success"] = "申請已核准。";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Reject(long id, string comment)
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        try
        {
            await _requestService.RejectRequestAsync(id, userId, comment);
            TempData["Success"] = "申請已退回。";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Index");
    }
}
