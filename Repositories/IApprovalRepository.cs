using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface IApprovalRepository
{
    Task<IEnumerable<ApprovalLog>> GetByRequestIdAsync(long requestId);
    Task<long> CreateAsync(ApprovalLog log);
}
