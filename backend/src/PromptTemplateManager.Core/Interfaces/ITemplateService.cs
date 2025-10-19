using PromptTemplateManager.Core.Entities;

namespace PromptTemplateManager.Core.Interfaces;

public interface ITemplateService
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Template> Items, int TotalCount)> GetAllAsync(Guid? folderId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Template> CreateAsync(string name, string content, Guid? folderId, CancellationToken cancellationToken = default);
    Task<Template> UpdateAsync(Guid id, string name, string content, Guid? folderId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Template> Items, int TotalCount)> SearchTemplatesAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
}
