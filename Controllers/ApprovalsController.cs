using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Repositories;
using SheetFlow.Services;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ApprovalsController : Controller
{
    private readonly IFormRequestRepository _requestRepo;
    private readonly IFormRequestService _requestService;

    public ApprovalsController(
        IFormRequestRepository requestRepo,
        IFormRequestService requestService)
    {
        _requestRepo = requestRepo;
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var requests = await _requestRepo.GetPendingAsync();
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
