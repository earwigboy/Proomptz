using PromptTemplateManager.Core.Entities;

namespace PromptTemplateManager.Core.Interfaces;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> GetAllAsync(Guid? folderId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(Guid? folderId, CancellationToken cancellationToken = default);
    Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default);
    Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameInFolderAsync(string name, Guid? folderId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Template>> SearchTemplatesAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetSearchCountAsync(string query, CancellationToken cancellationToken = default);
}
