using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Repositories;
using SheetFlow.Services;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize]
public class FormsController : Controller
{
    private readonly IFormTemplateRepository _templateRepo;
    private readonly IFormRequestService _requestService;
    private readonly IApprovalWorkflowRepository _workflowRepo;

    public FormsController(
        IFormTemplateRepository templateRepo,
        IFormRequestService requestService,
        IApprovalWorkflowRepository workflowRepo)
    {
        _templateRepo = templateRepo;
        _requestService = requestService;
        _workflowRepo = workflowRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await _templateRepo.GetActiveAsync();
        return View(templates);
    }

    [HttpGet]
    public async Task<IActionResult> Submit(long id)
    {
        var template = await _templateRepo.GetByIdAsync(id);
        if (template == null || !template.IsActive)
            return NotFound("表單不存在或已停用");

        var fields = await _templateRepo.GetVisibleFieldsAsync(id);
        var vm = new DynamicFormSubmitViewModel
        {
            FormTemplateId = id,
            FormName = template.Name,
            Fields = fields.Select(f => new DynamicFieldViewModel
            {
                FieldId = f.Id,
                FieldKey = f.FieldKey,
                FieldName = f.FieldName,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                SortOrder = f.SortOrder,
                OptionsJson = f.OptionsJson,
                DefaultValue = f.DefaultValue,
                Value = f.DefaultValue
            }).ToList()
        };

        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var dependencies = await _workflowRepo.GetDependenciesByTemplateAsync(id);
        var depList = dependencies.ToList();
        if (depList.Any())
        {
            var dep = depList.First();
            var approvedRequests = await _workflowRepo.GetApprovedRequestsByUserAndTemplateAsync(userId, dep.RequiredTemplateId);
            vm.AvailablePrerequisiteRequests = approvedRequests.ToList();
            vm.PrerequisiteTemplateName = dep.RequiredTemplateName;
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(long id, IFormCollection form, long? prerequisiteRequestId)
    {
        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var formValues = new Dictionary<string, string>();

        var fields = await _templateRepo.GetVisibleFieldsAsync(id);
        foreach (var field in fields)
        {
            var key = $"field_{field.Id}";
            formValues[key] = form[key].FirstOrDefault() ?? string.Empty;
        }

        try
        {
            var requestId = await _requestService.SubmitRequestAsync(id, userId, formValues, prerequisiteRequestId);
            TempData["Success"] = "申請已送出。";
            return RedirectToAction("Detail", "Requests", new { id = requestId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var template = await _templateRepo.GetByIdAsync(id);
            var vm = new DynamicFormSubmitViewModel
            {
                FormTemplateId = id,
                FormName = template?.Name ?? "",
                Fields = fields.Select(f => new DynamicFieldViewModel
                {
                    FieldId = f.Id,
                    FieldKey = f.FieldKey,
                    FieldName = f.FieldName,
                    FieldType = f.FieldType,
                    IsRequired = f.IsRequired,
                    SortOrder = f.SortOrder,
                    OptionsJson = f.OptionsJson,
                    DefaultValue = f.DefaultValue,
                    Value = formValues.GetValueOrDefault($"field_{f.Id}")
                }).ToList()
            };

            var dependencies = await _workflowRepo.GetDependenciesByTemplateAsync(id);
            var depList = dependencies.ToList();
            if (depList.Any())
            {
                var dep = depList.First();
                var approvedRequests = await _workflowRepo.GetApprovedRequestsByUserAndTemplateAsync(userId, dep.RequiredTemplateId);
                vm.AvailablePrerequisiteRequests = approvedRequests.ToList();
                vm.PrerequisiteTemplateName = dep.RequiredTemplateName;
            }

            return View(vm);
        }
    }
}
