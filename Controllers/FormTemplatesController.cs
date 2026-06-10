using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Models;
using SheetFlow.Repositories;
using SheetFlow.Services;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize(Roles = "Admin")]
public class FormTemplatesController : Controller
{
    private readonly IFormTemplateRepository _templateRepo;
    private readonly ExcelTemplateParser _parser;
    private readonly IWebHostEnvironment _env;

    public FormTemplatesController(
        IFormTemplateRepository templateRepo,
        ExcelTemplateParser parser,
        IWebHostEnvironment env)
    {
        _templateRepo = templateRepo;
        _parser = parser;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await _templateRepo.GetAllAsync();
        return View(templates);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new FormTemplateCreateViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(FormTemplateCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = long.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var now = DateTime.UtcNow;

        string? excelPath = null;
        if (model.ExcelFile != null)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "templates");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}_{model.ExcelFile.FileName}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await model.ExcelFile.CopyToAsync(stream);
            excelPath = $"/uploads/templates/{fileName}";

            var template = new FormTemplate
            {
                Name = model.Name,
                Description = model.Description,
                ExcelFilePath = excelPath,
                IsActive = model.IsActive,
                CreatedBy = userId,
                CreatedAt = now,
                UpdatedAt = now
            };

            var templateId = await _templateRepo.CreateAsync(template);

            stream.Position = 0;
            var parsedFields = _parser.Parse(stream);

            var sortOrder = 1;
            foreach (var pf in parsedFields)
            {
                var field = new FormTemplateField
                {
                    FormTemplateId = templateId,
                    FieldName = pf.FieldName,
                    FieldKey = pf.FieldKey,
                    FieldType = pf.FieldType,
                    IsRequired = pf.IsRequired,
                    SortOrder = sortOrder++,
                    IsVisible = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await _templateRepo.CreateFieldAsync(field);
            }

            TempData["Success"] = "表單建立成功，請設定欄位型態。";
            return RedirectToAction("EditFields", new { id = templateId });
        }

        var tmpl = new FormTemplate
        {
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive,
            CreatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now
        };
        await _templateRepo.CreateAsync(tmpl);

        TempData["Success"] = "表單建立成功。";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> EditFields(long id)
    {
        var template = await _templateRepo.GetByIdAsync(id);
        if (template == null) return NotFound();

        var fields = await _templateRepo.GetFieldsAsync(id);
        var vm = new FormFieldSettingsViewModel
        {
            FormTemplateId = id,
            FormName = template.Name,
            Fields = fields.Select(f => new FormFieldSettingViewModel
            {
                Id = f.Id,
                FormTemplateId = f.FormTemplateId,
                FieldName = f.FieldName,
                FieldKey = f.FieldKey,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                SortOrder = f.SortOrder,
                OptionsText = f.OptionsJson,
                DefaultValue = f.DefaultValue,
                IsVisible = f.IsVisible
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditFields(FormFieldSettingsViewModel model)
    {
        var now = DateTime.UtcNow;

        foreach (var f in model.Fields)
        {
            var field = new FormTemplateField
            {
                Id = f.Id,
                FormTemplateId = model.FormTemplateId,
                FieldName = f.FieldName,
                FieldKey = f.FieldKey,
                FieldType = f.FieldType,
                IsRequired = f.IsRequired,
                SortOrder = f.SortOrder,
                OptionsJson = f.FieldType == "Select" ? f.OptionsText : null,
                DefaultValue = f.DefaultValue,
                IsVisible = f.IsVisible,
                UpdatedAt = now
            };
            await _templateRepo.UpdateFieldAsync(field);
        }

        TempData["Success"] = "欄位設定已儲存。";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(long id)
    {
        var template = await _templateRepo.GetByIdAsync(id);
        if (template == null) return NotFound();

        template.IsActive = !template.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        await _templateRepo.UpdateAsync(template);

        TempData["Success"] = template.IsActive ? "表單已啟用。" : "表單已停用。";
        return RedirectToAction("Index");
    }
}
