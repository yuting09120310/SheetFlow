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

    public FormsController(
        IFormTemplateRepository templateRepo,
        IFormRequestService requestService)
    {
        _templateRepo = templateRepo;
        _requestService = requestService;
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

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(long id, IFormCollection form)
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
            var requestId = await _requestService.SubmitRequestAsync(id, userId, formValues);
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
            return View(vm);
        }
    }
}
