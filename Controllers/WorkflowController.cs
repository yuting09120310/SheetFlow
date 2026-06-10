using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheetFlow.Models;
using SheetFlow.Repositories;
using SheetFlow.ViewModels;

namespace SheetFlow.Controllers;

[Authorize(Roles = "Admin")]
public class WorkflowController : Controller
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly IFormTemplateRepository _templateRepo;
    private readonly IUserRepository _userRepo;

    public WorkflowController(
        IApprovalWorkflowRepository workflowRepo,
        IFormTemplateRepository templateRepo,
        IUserRepository userRepo)
    {
        _workflowRepo = workflowRepo;
        _templateRepo = templateRepo;
        _userRepo = userRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await _templateRepo.GetActiveAsync();
        var workflows = new List<ApprovalWorkflow>();
        foreach (var t in templates)
        {
            var ws = await _workflowRepo.GetByTemplateIdAsync(t.Id);
            workflows.AddRange(ws);
        }
        return View(workflows);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long? id)
    {
        var vm = new WorkflowEditViewModel
        {
            Templates = (await _templateRepo.GetActiveAsync()).ToList(),
            Departments = GetDepartments(),
            Users = (await _userRepo.GetAllAsync()).ToList(),
        };

        if (id.HasValue && id.Value > 0)
        {
            var workflow = await _workflowRepo.GetByIdAsync(id.Value);
            if (workflow == null) return NotFound();
            vm.Workflow = workflow;
            vm.ExistingWorkflows = (await _workflowRepo.GetByTemplateIdAsync(workflow.FormTemplateId))
                .Where(w => w.Id != id.Value).ToList();
        }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Save(long? id, string name, long formTemplateId, string? department)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "請輸入簽呈名稱";
            return RedirectToAction("Edit", new { id });
        }

        long workflowId;
        if (id.HasValue && id.Value > 0)
        {
            var workflow = await _workflowRepo.GetByIdAsync(id.Value);
            if (workflow == null) return NotFound();
            workflow.Name = name;
            workflow.FormTemplateId = formTemplateId;
            workflow.Department = string.IsNullOrWhiteSpace(department) ? null : department;
            await _workflowRepo.UpdateAsync(workflow);
            workflowId = id.Value;
        }
        else
        {
            var workflow = new ApprovalWorkflow
            {
                Name = name,
                FormTemplateId = formTemplateId,
                Department = string.IsNullOrWhiteSpace(department) ? null : department,
                IsActive = true
            };
            workflowId = await _workflowRepo.CreateAsync(workflow);
        }

        return RedirectToAction("Edit", new { id = workflowId });
    }

    [HttpPost]
    public async Task<IActionResult> AddStep(long workflowId, string approverType, string? approverTarget)
    {
        var workflow = await _workflowRepo.GetByIdAsync(workflowId);
        if (workflow == null) return NotFound();

        var maxOrder = workflow.Steps.Any() ? workflow.Steps.Max(s => s.StepOrder) : 0;
        var step = new ApprovalWorkflowStep
        {
            ApprovalWorkflowId = workflowId,
            StepOrder = maxOrder + 1,
            ApproverType = approverType,
            ApproverTarget = approverTarget
        };
        await _workflowRepo.CreateStepAsync(step);

        return RedirectToAction("Edit", new { id = workflowId });
    }

    [HttpPost]
    public async Task<IActionResult> ReorderSteps(long workflowId, [FromBody] List<StepReorderItem> items)
    {
        var orders = items.ToDictionary(i => i.Id, i => i.Order);
        await _workflowRepo.ReorderStepsAsync(workflowId, orders);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStep(long workflowId, long stepId)
    {
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(
            "Server=neko-meow.com;Database=SheetFlow;User ID=sa;Password=p2lALhay8gxLKkv4;TrustServerCertificate=True");
        await conn.OpenAsync();
        await conn.ExecuteAsync("DELETE FROM [approval_workflow_steps] WHERE [id] = @Id AND [approval_workflow_id] = @WorkflowId",
            new { Id = stepId, WorkflowId = workflowId });

        var steps = await conn.QueryAsync<ApprovalWorkflowStep>(
            "SELECT * FROM [approval_workflow_steps] WHERE [approval_workflow_id] = @WorkflowId ORDER BY [step_order]",
            new { WorkflowId = workflowId });

        int order = 1;
        foreach (var step in steps)
        {
            await conn.ExecuteAsync("UPDATE [approval_workflow_steps] SET [step_order] = @Order WHERE [id] = @Id",
                new { Order = order, Id = step.Id });
            order++;
        }

        return RedirectToAction("Edit", new { id = workflowId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        await _workflowRepo.DeleteAsync(id);
        TempData["Success"] = "簽呈流程已刪除";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Dependencies(long templateId)
    {
        var template = await _templateRepo.GetByIdAsync(templateId);
        if (template == null) return NotFound();

        var dependencies = await _workflowRepo.GetDependenciesByTemplateAsync(templateId);
        var allTemplates = await _templateRepo.GetActiveAsync();

        ViewBag.Template = template;
        ViewBag.AllTemplates = allTemplates.Where(t => t.Id != templateId).ToList();
        ViewBag.Dependencies = dependencies.ToList();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddDependency(long templateId, long requiredTemplateId)
    {
        var dep = new FormTemplateDependency
        {
            FormTemplateId = templateId,
            RequiredTemplateId = requiredTemplateId,
            RequiredStatus = "Approved"
        };
        await _workflowRepo.CreateDependencyAsync(dep);
        TempData["Success"] = "已新增前置表單依賴";
        return RedirectToAction("Dependencies", new { templateId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDependency(long templateId, long id)
    {
        await _workflowRepo.DeleteDependencyAsync(id);
        TempData["Success"] = "已刪除前置表單依賴";
        return RedirectToAction("Dependencies", new { templateId });
    }

    private List<string> GetDepartments()
    {
        return new List<string> { "會計部", "財務部", "食安部", "資訊部", "企劃部" };
    }
}

public class StepReorderItem
{
    public long Id { get; set; }
    public int Order { get; set; }
}
