using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface IEmployeeProfileRepository
{
    Task<EmployeeProfile?> GetByUsernameAsync(string username);
    Task<IEnumerable<EmployeeProfile>> GetAllAsync();
    Task UpdateAsync(EmployeeProfile profile);
}
