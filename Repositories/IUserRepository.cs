using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllAsync();
    Task<long> CreateAsync(User user);
    Task UpdateAsync(User user);
}
