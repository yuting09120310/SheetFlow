using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperDbContext _db;

    public UserRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM [users] WHERE [id] = @Id", new { Id = id });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM [users] WHERE [username] = @Username", new { Username = username });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<User>("SELECT * FROM [users] ORDER BY [display_name]");
    }

    public async Task<long> CreateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [users] ([username],[password_hash],[display_name],[email],[line_user_id],[role],[is_active],[created_at],[updated_at])
                    VALUES (@Username,@PasswordHash,@DisplayName,@Email,@LineUserId,@Role,@IsActive,@CreatedAt,@UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, user);
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = _db.CreateConnection();
        var sql = @"UPDATE [users] SET [display_name]=@DisplayName,[email]=@Email,
                    [line_user_id]=@LineUserId,[role]=@Role,[is_active]=@IsActive,
                    [updated_at]=@UpdatedAt WHERE [id]=@Id";
        await conn.ExecuteAsync(sql, user);
    }
}
