using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Repositories;
using SheetFlow.Services;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize]
public class RequestsController : Controller
{
    private readonly IFormRequestRepository _requestRepo;
    private readonly IFormTemplateRepository _templateRepo;
    private readonly IFormRequestService _requestService;

    public RequestsController(
        IFormRequestRepository requestRepo,
        IFormTemplateRepository templateRepo,
        IFormRequestService requestService)
    {
        _requestRepo = requestRepo;
        _templateRepo = templateRepo;
        _requestService = requestService;
    }

    [HttpGet]
    public async Task<IActionResult> MyRequests(string? status)
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var requests = await _requestRepo.GetByApplicantAsync(userId, status);
        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

        var vm = await _requestService.GetRequestDetailAsync(id);

        if (role != "Admin" && role != "Manager" && vm.Request.ApplicantId != userId)
            return Forbid();

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Resubmit(long id, IFormCollection form)
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var formValues = new Dictionary<string, string>();

        var fields = await _templateRepo.GetVisibleFieldsAsync(
            (await _requestRepo.GetByIdAsync(id))?.FormTemplateId ?? 0);

        foreach (var field in fields)
        {
            var key = $"field_{field.Id}";
            formValues[key] = form[key].FirstOrDefault() ?? string.Empty;
        }

        try
        {
            await _requestService.ResubmitRequestAsync(id, userId, formValues);
            TempData["Success"] = "申請已重新送出。";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Detail", new { id });
    }
}
