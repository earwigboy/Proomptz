using PromptTemplateManager.Application.DTOs;

namespace PromptTemplateManager.Application.Services;

public interface IFolderService
{
    Task<FolderTreeResponse> GetFolderTreeAsync(CancellationToken cancellationToken = default);
    Task<FolderDetailsResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FolderResponse>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default);
    Task<FolderResponse> CreateAsync(CreateFolderRequest request, CancellationToken cancellationToken = default);
    Task<FolderResponse> UpdateAsync(Guid id, UpdateFolderRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
