using PromptTemplateManager.Core.Entities;

namespace PromptTemplateManager.Core.Interfaces;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Folder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Folder>> GetTreeAsync(CancellationToken cancellationToken = default);
    Task<List<Folder>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<Folder?> GetByNameAndParentAsync(string name, Guid? parentId, CancellationToken cancellationToken = default);
    Task<bool> HasCircularReferenceAsync(Guid folderId, Guid? newParentId, CancellationToken cancellationToken = default);
    Task<int> GetDepthAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<bool> IsEmptyAsync(Guid folderId, CancellationToken cancellationToken = default);
    Task<Folder> CreateAsync(Folder folder, CancellationToken cancellationToken = default);
    Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
