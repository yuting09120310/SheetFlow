using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface IFormRequestRepository
{
    Task<FormRequest?> GetByIdAsync(long id);
    Task<IEnumerable<FormRequest>> GetByApplicantAsync(long applicantId, string? status = null);
    Task<IEnumerable<FormRequest>> GetPendingAsync();
    Task<IEnumerable<FormRequest>> SearchAsync(long? templateId, long? applicantId, DateTime? startDate, DateTime? endDate, string? status);
    Task<long> CreateAsync(FormRequest request);
    Task UpdateStatusAsync(long id, string status);
    Task UpdateAsync(FormRequest request);

    Task<IEnumerable<FormRequestValue>> GetValuesAsync(long requestId);
    Task<long> CreateValueAsync(FormRequestValue value);
    Task DeleteValuesAsync(long requestId);
}
