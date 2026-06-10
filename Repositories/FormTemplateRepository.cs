using Dapper;
using SheetFlow.Infrastructure;
using SheetFlow.Models;

namespace SheetFlow.Repositories;

public class FormTemplateRepository : IFormTemplateRepository
{
    private readonly DapperDbContext _db;

    public FormTemplateRepository(DapperDbContext db)
    {
        _db = db;
    }

    public async Task<FormTemplate?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<FormTemplate>(
            "SELECT * FROM [form_templates] WHERE [id] = @Id", new { Id = id });
    }

    public async Task<IEnumerable<FormTemplate>> GetAllAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<FormTemplate>(
            "SELECT * FROM [form_templates] ORDER BY [created_at] DESC");
    }

    public async Task<IEnumerable<FormTemplate>> GetActiveAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<FormTemplate>(
            "SELECT * FROM [form_templates] WHERE [is_active] = 1 ORDER BY [name]");
    }

    public async Task<long> CreateAsync(FormTemplate template)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [form_templates] ([name],[description],[excel_file_path],[is_active],[created_by],[created_at],[updated_at])
                    VALUES (@Name,@Description,@ExcelFilePath,@IsActive,@CreatedBy,@CreatedAt,@UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, template);
    }

    public async Task UpdateAsync(FormTemplate template)
    {
        using var conn = _db.CreateConnection();
        var sql = @"UPDATE [form_templates] SET [name]=@Name,[description]=@Description,
                    [excel_file_path]=@ExcelFilePath,[is_active]=@IsActive,
                    [updated_at]=@UpdatedAt WHERE [id]=@Id";
        await conn.ExecuteAsync(sql, template);
    }

    public async Task<IEnumerable<FormTemplateField>> GetFieldsAsync(long templateId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<FormTemplateField>(
            "SELECT * FROM [form_template_fields] WHERE [form_template_id] = @Id ORDER BY [sort_order]",
            new { Id = templateId });
    }

    public async Task<IEnumerable<FormTemplateField>> GetVisibleFieldsAsync(long templateId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<FormTemplateField>(
            "SELECT * FROM [form_template_fields] WHERE [form_template_id] = @Id AND [is_visible] = 1 ORDER BY [sort_order]",
            new { Id = templateId });
    }

    public async Task<FormTemplateField?> GetFieldByIdAsync(long fieldId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<FormTemplateField>(
            "SELECT * FROM [form_template_fields] WHERE [id] = @Id", new { Id = fieldId });
    }

    public async Task DeleteFieldsAsync(long templateId)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "DELETE FROM [form_template_fields] WHERE [form_template_id] = @Id",
            new { Id = templateId });
    }

    public async Task<long> CreateFieldAsync(FormTemplateField field)
    {
        using var conn = _db.CreateConnection();
        var sql = @"INSERT INTO [form_template_fields] ([form_template_id],[field_name],[field_key],[field_type],[is_required],[sort_order],[options_json],[default_value],[is_visible],[created_at],[updated_at])
                    VALUES (@FormTemplateId,@FieldName,@FieldKey,@FieldType,@IsRequired,@SortOrder,@OptionsJson,@DefaultValue,@IsVisible,@CreatedAt,@UpdatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT)";
        return await conn.ExecuteScalarAsync<long>(sql, field);
    }

    public async Task UpdateFieldAsync(FormTemplateField field)
    {
        using var conn = _db.CreateConnection();
        var sql = @"UPDATE [form_template_fields] SET [field_name]=@FieldName,[field_type]=@FieldType,
                    [is_required]=@IsRequired,[sort_order]=@SortOrder,[options_json]=@OptionsJson,
                    [default_value]=@DefaultValue,[is_visible]=@IsVisible,[updated_at]=@UpdatedAt
                    WHERE [id]=@Id";
        await conn.ExecuteAsync(sql, field);
    }
}
