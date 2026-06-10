using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Repositories;
using SheetFlow.Services;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ExportsController : Controller
{
    private readonly IFormTemplateRepository _templateRepo;
    private readonly ExcelExportService _exportService;

    public ExportsController(
        IFormTemplateRepository templateRepo,
        ExcelExportService exportService)
    {
        _templateRepo = templateRepo;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new ExportViewModel
        {
            Templates = (await _templateRepo.GetAllAsync()).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ExportExcel(long? formTemplateId, DateTime? startDate, DateTime? endDate, string? status)
    {
        var bytes = await _exportService.ExportMultipleAsync(formTemplateId, startDate, endDate, status);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SheetFlow_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ExportSingle(long id)
    {
        var bytes = await _exportService.ExportSingleAsync(id);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SheetFlow_Request_{id}.xlsx");
    }
}
