using SheetFlow.Models;
using SheetFlow.Repositories;
using SheetFlow.ViewModels;

namespace SheetFlow.Services;

public class FormRequestService : IFormRequestService
{
    private readonly IFormTemplateRepository _templateRepo;
    private readonly IFormRequestRepository _requestRepo;
    private readonly IApprovalRepository _approvalRepo;
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly IUserRepository _userRepo;
    private readonly INotificationService _notifService;

    public FormRequestService(
        IFormTemplateRepository templateRepo,
        IFormRequestRepository requestRepo,
        IApprovalRepository approvalRepo,
        IApprovalWorkflowRepository workflowRepo,
        IUserRepository userRepo,
        INotificationService notifService)
    {
        _templateRepo = templateRepo;
        _requestRepo = requestRepo;
        _approvalRepo = approvalRepo;
        _workflowRepo = workflowRepo;
        _userRepo = userRepo;
        _notifService = notifService;
    }

    public async Task<long> SubmitRequestAsync(long templateId, long applicantId, Dictionary<string, string> formValues, long? prerequisiteRequestId = null)
    {
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new Exception("表單不存在");

        var applicant = await _userRepo.GetByIdAsync(applicantId)
            ?? throw new Exception("申請人不存在");

        var dependencies = await _workflowRepo.GetDependenciesByTemplateAsync(templateId);
        var depList = dependencies.ToList();
        if (depList.Any() && !prerequisiteRequestId.HasValue)
        {
            var depNames = string.Join("、", depList.Select(d => d.RequiredTemplateName));
            throw new Exception($"此表單需要先有已審核完成的「{depNames}」才能提交");
        }

        var now = DateTime.UtcNow;
        var request = new FormRequest
        {
            RequestNo = GenerateRequestNo(template.Name),
            FormTemplateId = templateId,
            ApplicantId = applicantId,
            Status = "Pending",
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        var requestId = await _requestRepo.CreateAsync(request);

        var fields = await _templateRepo.GetVisibleFieldsAsync(templateId);
        foreach (var field in fields)
        {
            formValues.TryGetValue($"field_{field.Id}", out var value);
            var rv = new FormRequestValue
            {
                FormRequestId = requestId,
                FieldId = field.Id,
                FieldKey = field.FieldKey,
                FieldName = field.FieldName,
                FieldValue = value ?? string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _requestRepo.CreateValueAsync(rv);
        }

        if (prerequisiteRequestId.HasValue)
        {
            await _workflowRepo.CreateRequestDependencyAsync(new FormRequestDependency
            {
                FormRequestId = requestId,
                RequiredRequestId = prerequisiteRequestId.Value
            });
        }

        var workflow = await _workflowRepo.GetByTemplateAndDepartmentAsync(templateId, applicant.Department);
        await CreateApprovalStepsAsync(requestId, workflow, applicant);

        var log = new ApprovalLog
        {
            FormRequestId = requestId,
            Action = "Submit",
            ActorId = applicantId,
            CreatedAt = now
        };
        await _approvalRepo.CreateAsync(log);

        await NotifyNextApproverAsync(requestId, template.Name);

        return requestId;
    }

    public async Task ResubmitRequestAsync(long requestId, long applicantId, Dictionary<string, string> formValues)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");

        if (request.ApplicantId != applicantId)
            throw new Exception("無權限操作此申請單");

        if (request.Status != "Rejected")
            throw new Exception("只有已退回的申請單可以重新送出");

        var now = DateTime.UtcNow;

        await _requestRepo.DeleteValuesAsync(requestId);
        var fields = await _templateRepo.GetVisibleFieldsAsync(request.FormTemplateId);
        foreach (var field in fields)
        {
            formValues.TryGetValue($"field_{field.Id}", out var value);
            var rv = new FormRequestValue
            {
                FormRequestId = requestId,
                FieldId = field.Id,
                FieldKey = field.FieldKey,
                FieldName = field.FieldName,
                FieldValue = value ?? string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _requestRepo.CreateValueAsync(rv);
        }

        var applicant = await _userRepo.GetByIdAsync(request.ApplicantId)
            ?? throw new Exception("申請人不存在");

        var stepInstances = await _workflowRepo.GetStepInstancesByRequestAsync(requestId);
        var stepList = stepInstances.ToList();
        if (stepList.Any())
        {
            foreach (var step in stepList)
            {
                step.Status = "Pending";
                step.ApprovedAt = null;
                step.RejectedAt = null;
                step.Comment = null;
                step.UpdatedAt = now;
                await _workflowRepo.UpdateStepInstanceAsync(step);
            }

            var firstStep = stepList.OrderBy(s => s.StepOrder).First();
            firstStep.AssignedUserId = await ResolveApproverAsync(firstStep.ApproverType, firstStep.ApproverTarget, applicant);
            await _workflowRepo.UpdateStepInstanceAsync(firstStep);
        }

        request.Status = "Pending";
        request.SubmittedAt = now;
        request.RejectedAt = null;
        request.UpdatedAt = now;
        await _requestRepo.UpdateAsync(request);

        var log = new ApprovalLog
        {
            FormRequestId = requestId,
            Action = "Resubmit",
            ActorId = applicantId,
            Comment = "重新送出",
            CreatedAt = now
        };
        await _approvalRepo.CreateAsync(log);

        var template = await _templateRepo.GetByIdAsync(request.FormTemplateId);
        await NotifyNextApproverAsync(requestId, template?.Name ?? "");
    }

    public async Task ApproveRequestAsync(long requestId, long approverId, string? comment)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");

        var currentStep = await _workflowRepo.GetCurrentStepInstanceAsync(requestId);
        if (currentStep == null)
            throw new Exception("此申請單沒有待處理的簽核步驟");

        if (currentStep.AssignedUserId != approverId)
        {
            var approver = await _userRepo.GetByIdAsync(approverId);
            if (approver?.Role != "Admin")
                throw new Exception("您不是此步驟的簽核者");
        }

        var now = DateTime.UtcNow;
        currentStep.Status = "Approved";
        currentStep.ApprovedAt = now;
        currentStep.Comment = comment;
        currentStep.UpdatedAt = now;
        await _workflowRepo.UpdateStepInstanceAsync(currentStep);

        var log = new ApprovalLog
        {
            FormRequestId = requestId,
            Action = "Approve",
            ActorId = approverId,
            Comment = comment,
            CreatedAt = now
        };
        await _approvalRepo.CreateAsync(log);

        var nextStep = await _workflowRepo.GetCurrentStepInstanceAsync(requestId);
        if (nextStep != null)
        {
            var applicant = await _userRepo.GetByIdAsync(request.ApplicantId);
            nextStep.AssignedUserId = await ResolveApproverAsync(nextStep.ApproverType, nextStep.ApproverTarget, applicant!);
            await _workflowRepo.UpdateStepInstanceAsync(nextStep);
            if (nextStep.AssignedUserId.HasValue)
            {
                var template = await _templateRepo.GetByIdAsync(request.FormTemplateId);
                await NotifySpecificUserAsync(nextStep.AssignedUserId.Value, "待簽核通知",
                    $"申請單 {request.RequestNo} 已輪到您簽核。");
            }
        }
        else
        {
            request.Status = "Approved";
            request.ApprovedAt = now;
            request.UpdatedAt = now;
            await _requestRepo.UpdateAsync(request);

            var template = await _templateRepo.GetByIdAsync(request.FormTemplateId);
            await _notifService.NotifyUserAsync(request.ApplicantId, "申請已核准",
                $"您的申請「{template?.Name}」({request.RequestNo}) 已核准。");
        }
    }

    public async Task RejectRequestAsync(long requestId, long approverId, string comment)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");

        var currentStep = await _workflowRepo.GetCurrentStepInstanceAsync(requestId);
        if (currentStep == null)
            throw new Exception("此申請單沒有待處理的簽核步驟");

        if (currentStep.AssignedUserId != approverId)
        {
            var approver = await _userRepo.GetByIdAsync(approverId);
            if (approver?.Role != "Admin")
                throw new Exception("您不是此步驟的簽核者");
        }

        var now = DateTime.UtcNow;
        currentStep.Status = "Rejected";
        currentStep.RejectedAt = now;
        currentStep.Comment = comment;
        currentStep.UpdatedAt = now;
        await _workflowRepo.UpdateStepInstanceAsync(currentStep);

        // Mark all subsequent pending steps as Skipped
        var allSteps = await _workflowRepo.GetStepInstancesByRequestAsync(requestId);
        foreach (var step in allSteps.Where(s => s.StepOrder > currentStep.StepOrder && s.Status == "Pending"))
        {
            step.Status = "Skipped";
            step.UpdatedAt = now;
            await _workflowRepo.UpdateStepInstanceAsync(step);
        }

        request.Status = "Rejected";
        request.RejectedAt = now;
        request.UpdatedAt = now;
        await _requestRepo.UpdateAsync(request);

        var log = new ApprovalLog
        {
            FormRequestId = requestId,
            Action = "Reject",
            ActorId = approverId,
            Comment = comment,
            CreatedAt = now
        };
        await _approvalRepo.CreateAsync(log);

        var template = await _templateRepo.GetByIdAsync(request.FormTemplateId);
        await _notifService.NotifyUserAsync(request.ApplicantId, "申請已退回",
            $"您的申請「{template?.Name}」({request.RequestNo}) 已退回。\n退回原因：{comment}");
    }

    public async Task<RequestDetailViewModel> GetRequestDetailAsync(long requestId)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");

        var values = (await _requestRepo.GetValuesAsync(requestId)).ToList();
        var logs = (await _approvalRepo.GetByRequestIdAsync(requestId)).ToList();
        var steps = (await _workflowRepo.GetStepInstancesByRequestAsync(requestId)).ToList();
        var fields = (await _templateRepo.GetVisibleFieldsAsync(request.FormTemplateId)).ToList();
        var deps = (await _workflowRepo.GetDependenciesByRequestAsync(requestId)).ToList();

        return new RequestDetailViewModel
        {
            Request = request,
            Values = values,
            ApprovalLogs = logs,
            StepInstances = steps,
            Fields = fields,
            Dependencies = deps
        };
    }

    private async Task CreateApprovalStepsAsync(long requestId, ApprovalWorkflow? workflow, User applicant)
    {
        if (workflow == null || !workflow.Steps.Any())
        {
            var adminUsers = await _userRepo.GetAllAsync();
            var admin = adminUsers.FirstOrDefault(u => u.Role == "Admin");
            var instance = new ApprovalStepInstance
            {
                FormRequestId = requestId,
                StepOrder = 1,
                ApproverType = "Role",
                ApproverTarget = "Admin",
                AssignedUserId = admin?.Id,
                Status = "Pending"
            };
            await _workflowRepo.CreateStepInstanceAsync(instance);
            return;
        }

        var resolvedSteps = new List<(ApprovalWorkflowStep Step, long? AssignedUserId)>();

        foreach (var step in workflow.Steps.OrderBy(s => s.StepOrder))
        {
            var assignedUserId = await ResolveApproverAsync(step.ApproverType, step.ApproverTarget, applicant);
            resolvedSteps.Add((step, assignedUserId));
        }

        var dedupedSteps = RemoveDuplicateConsecutiveApprover(resolvedSteps);

        int order = 1;
        foreach (var (step, assignedUserId) in dedupedSteps)
        {
            var instance = new ApprovalStepInstance
            {
                FormRequestId = requestId,
                StepOrder = order,
                ApproverType = step.ApproverType,
                ApproverTarget = step.ApproverTarget,
                AssignedUserId = assignedUserId,
                Status = "Pending"
            };
            await _workflowRepo.CreateStepInstanceAsync(instance);
            order++;
        }
    }

    private List<(ApprovalWorkflowStep Step, long? AssignedUserId)> RemoveDuplicateConsecutiveApprover(
        List<(ApprovalWorkflowStep Step, long? AssignedUserId)> steps)
    {
        var result = new List<(ApprovalWorkflowStep Step, long? AssignedUserId)>();
        long? lastUserId = null;

        foreach (var (step, userId) in steps)
        {
            if (userId != null && userId == lastUserId)
                continue;

            result.Add((step, userId));
            lastUserId = userId;
        }

        return result;
    }

    private async Task<long?> ResolveApproverAsync(string approverType, string? approverTarget, User applicant)
    {
        switch (approverType)
        {
            case "ApplicantDepartmentManager":
                if (!string.IsNullOrEmpty(applicant.Department))
                {
                    var mgr = await _userRepo.GetDepartmentManagerAsync(applicant.Department);
                    if (mgr != null)
                        return mgr.Id;
                }
                var allUsers1 = await _userRepo.GetAllAsync();
                return allUsers1.FirstOrDefault(u => u.Role == "Admin")?.Id;

            case "SpecificDepartmentManager":
                if (!string.IsNullOrEmpty(approverTarget))
                {
                    var mgr = await _userRepo.GetDepartmentManagerAsync(approverTarget);
                    if (mgr != null)
                        return mgr.Id;
                }
                var allUsers2 = await _userRepo.GetAllAsync();
                return allUsers2.FirstOrDefault(u => u.Role == "Admin")?.Id;

            case "SpecificUser":
                if (!string.IsNullOrEmpty(approverTarget))
                {
                    var users = await _userRepo.GetAllAsync();
                    var user = users.FirstOrDefault(u => u.DisplayName == approverTarget);
                    if (user != null)
                        return user.Id;
                }
                var allUsers3 = await _userRepo.GetAllAsync();
                return allUsers3.FirstOrDefault(u => u.Role == "Admin")?.Id;

            case "Role":
                if (approverTarget == "Admin")
                {
                    var users = await _userRepo.GetAllAsync();
                    return users.FirstOrDefault(u => u.Role == "Admin")?.Id;
                }
                else if (approverTarget == "Manager")
                {
                    if (!string.IsNullOrEmpty(applicant.Department))
                    {
                        var mgr = await _userRepo.GetDepartmentManagerAsync(applicant.Department);
                        if (mgr != null)
                            return mgr.Id;
                    }
                    var allUsers4 = await _userRepo.GetAllAsync();
                    return allUsers4.FirstOrDefault(u => u.Role == "Admin")?.Id;
                }
                break;
        }
        return null;
    }

    private async Task NotifyNextApproverAsync(long requestId, string formName)
    {
        var currentStep = await _workflowRepo.GetCurrentStepInstanceAsync(requestId);
        if (currentStep?.AssignedUserId.HasValue == true)
        {
            await NotifySpecificUserAsync(currentStep.AssignedUserId.Value, "待簽核通知",
                $"有一筆新的「{formName}」待您簽核。");
        }
        else
        {
            await _notifService.NotifyManagersAsync("待簽核通知",
                $"有一筆新的「{formName}」待簽核。");
        }
    }

    private async Task NotifySpecificUserAsync(long userId, string subject, string content)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            await _notifService.NotifyUserAsync(userId, subject, content);
        }
    }

    private string GenerateRequestNo(string formName)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        var prefix = formName.Length >= 2 ? formName[..2] : formName;
        return $"SF-{date}-{prefix}-{random}";
    }
}
