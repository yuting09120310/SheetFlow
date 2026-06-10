using SheetFlow.Models;

namespace SheetFlow.Repositories;

public interface IFormTemplateRepository
{
    Task<FormTemplate?> GetByIdAsync(long id);
    Task<IEnumerable<FormTemplate>> GetAllAsync();
    Task<IEnumerable<FormTemplate>> GetActiveAsync();
    Task<long> CreateAsync(FormTemplate template);
    Task UpdateAsync(FormTemplate template);
    Task<IEnumerable<FormTemplateField>> GetFieldsAsync(long templateId);
    Task<IEnumerable<FormTemplateField>> GetVisibleFieldsAsync(long templateId);
    Task<FormTemplateField?> GetFieldByIdAsync(long fieldId);
    Task DeleteFieldsAsync(long templateId);
    Task<long> CreateFieldAsync(FormTemplateField field);
    Task UpdateFieldAsync(FormTemplateField field);
}
