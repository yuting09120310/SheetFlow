using SheetFlow.Models;
using SheetFlow.ViewModels;

namespace SheetFlow.Services;

public interface IFormRequestService
{
    Task<long> SubmitRequestAsync(long templateId, long applicantId, Dictionary<string, string> formValues, long? prerequisiteRequestId = null);
    Task ResubmitRequestAsync(long requestId, long applicantId, Dictionary<string, string> formValues);
    Task ApproveRequestAsync(long requestId, long approverId, string? comment, string? signatureFieldKey = null, long? signatureFieldId = null, string? signatureValue = null);
    Task RejectRequestAsync(long requestId, long approverId, string comment);
    Task<RequestDetailViewModel> GetRequestDetailAsync(long requestId);
}
