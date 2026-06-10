using SheetFlow.Models;
using SheetFlow.Repositories;
using SheetFlow.ViewModels;

namespace SheetFlow.Services;

public class FormRequestService : IFormRequestService
{
    private readonly IFormTemplateRepository _templateRepo;
    private readonly IFormRequestRepository _requestRepo;
    private readonly IApprovalRepository _approvalRepo;
    private readonly INotificationService _notifService;

    public FormRequestService(
        IFormTemplateRepository templateRepo,
        IFormRequestRepository requestRepo,
        IApprovalRepository approvalRepo,
        INotificationService notifService)
    {
        _templateRepo = templateRepo;
        _requestRepo = requestRepo;
        _approvalRepo = approvalRepo;
        _notifService = notifService;
    }

    public async Task<long> SubmitRequestAsync(long templateId, long applicantId, Dictionary<string, string> formValues)
    {
        var template = await _templateRepo.GetByIdAsync(templateId)
            ?? throw new Exception("表單不存在");

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

        var log = new ApprovalLog
        {
            FormRequestId = requestId,
            Action = "Submit",
            ActorId = applicantId,
            CreatedAt = now
        };
        await _approvalRepo.CreateAsync(log);

        await _notifService.NotifyManagersAsync("新申請單", $"有一筆新的「{template.Name}」待審核。");

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
        await _notifService.NotifyManagersAsync("申請單重新送出", $"申請單 {request.RequestNo} 已重新送出。");
    }

    public async Task ApproveRequestAsync(long requestId, long approverId, string? comment)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");

        if (request.Status != "Pending" && request.Status != "Resubmitted")
            throw new Exception("此申請單無法核准");

        var now = DateTime.UtcNow;
        request.Status = "Approved";
        request.ApprovedAt = now;
        request.UpdatedAt = now;
        await _requestRepo.UpdateAsync(request);

        var log = new ApprovalLog
        {
            FormRequestId = requestId,
            Action = "Approve",
            ActorId = approverId,
            Comment = comment,
            CreatedAt = now
        };
        await _approvalRepo.CreateAsync(log);

        var template = await _templateRepo.GetByIdAsync(request.FormTemplateId);
        await _notifService.NotifyUserAsync(request.ApplicantId, "申請已核准", $"您的申請「{template?.Name}」({request.RequestNo}) 已核准。");
    }

    public async Task RejectRequestAsync(long requestId, long approverId, string comment)
    {
        var request = await _requestRepo.GetByIdAsync(requestId)
            ?? throw new Exception("申請單不存在");

        if (request.Status != "Pending" && request.Status != "Resubmitted")
            throw new Exception("此申請單無法退回");

        var now = DateTime.UtcNow;
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
        var fields = (await _templateRepo.GetVisibleFieldsAsync(request.FormTemplateId)).ToList();

        return new RequestDetailViewModel
        {
            Request = request,
            Values = values,
            ApprovalLogs = logs,
            Fields = fields
        };
    }

    private string GenerateRequestNo(string formName)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        var prefix = formName.Length >= 2 ? formName[..2] : formName;
        return $"SF-{date}-{prefix}-{random}";
    }
}
