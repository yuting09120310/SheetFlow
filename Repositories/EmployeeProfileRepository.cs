using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class EmployeeProfileRepository : IEmployeeProfileRepository
{
    private readonly DapperDbContext _db;

    public EmployeeProfileRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<EmployeeProfile?> GetByUsernameAsync(string username)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<EmployeeProfile>(
            "SELECT * FROM [hr_employee_profiles] WHERE [username] = @Username",
            new { Username = username });
    }

    public async Task<IEnumerable<EmployeeProfile>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<EmployeeProfile>(
            "SELECT * FROM [hr_employee_profiles] ORDER BY [full_name]");
    }

    public async Task UpdateAsync(EmployeeProfile profile)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE [hr_employee_profiles] SET [email]=@Email,[password]=@Password,[updated_at]=GETUTCDATE() WHERE [username]=@Username",
            profile);
    }
}
