using PromptTemplateManager.Application.DTOs;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Core.Exceptions;
using PromptTemplateManager.Core.Interfaces;

namespace PromptTemplateManager.Application.Services;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;
    private const int MaxDepth = 10;

    public FolderService(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<FolderTreeResponse> GetFolderTreeAsync(CancellationToken cancellationToken = default)
    {
        var allFolders = await _folderRepository.GetAllAsync(cancellationToken);
        var folderDict = allFolders.ToDictionary(f => f.Id);

        var rootFolders = allFolders
            .Where(f => f.ParentFolderId == null)
            .Select(f => BuildTreeNode(f, folderDict, 0))
            .OrderBy(n => n.Name)
            .ToList();

        return new FolderTreeResponse { RootFolders = rootFolders };
    }

    private FolderTreeNode BuildTreeNode(Folder folder, Dictionary<Guid, Folder> folderDict, int depth)
    {
        var node = new FolderTreeNode
        {
            Id = folder.Id,
            Name = folder.Name,
            Depth = depth,
            TemplateCount = folder.Templates?.Count ?? 0,
            ChildFolders = new List<FolderTreeNode>()
        };

        var children = folderDict.Values
            .Where(f => f.ParentFolderId == folder.Id)
            .OrderBy(f => f.Name)
            .Select(f => BuildTreeNode(f, folderDict, depth + 1))
            .ToList();

        node.ChildFolders = children;
        return node;
    }

    public async Task<FolderDetailsResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Folder with ID {id} not found");

        return new FolderDetailsResponse
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            ParentFolderName = folder.ParentFolder?.Name,
            TemplateCount = folder.Templates?.Count ?? 0,
            SubfolderCount = folder.ChildFolders?.Count ?? 0,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt
        };
    }

    public async Task<List<FolderResponse>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        var children = await _folderRepository.GetChildrenAsync(parentId, cancellationToken);

        return children.Select(f => new FolderResponse
        {
            Id = f.Id,
            Name = f.Name,
            ParentFolderId = f.ParentFolderId,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        }).ToList();
    }

    public async Task<FolderResponse> CreateAsync(CreateFolderRequest request, CancellationToken cancellationToken = default)
    {
        // Validate parent folder exists if specified
        if (request.ParentFolderId.HasValue)
        {
            var parentFolder = await _folderRepository.GetByIdAsync(request.ParentFolderId.Value, cancellationToken);
            if (parentFolder == null)
                throw new NotFoundException($"Parent folder with ID {request.ParentFolderId.Value} not found");

            // Check depth limit
            var parentDepth = await _folderRepository.GetDepthAsync(request.ParentFolderId.Value, cancellationToken);
            if (parentDepth >= MaxDepth)
                throw new ValidationException($"Maximum folder nesting depth ({MaxDepth}) exceeded");
        }

        // Check for duplicate name in same parent
        var existing = await _folderRepository.GetByNameAndParentAsync(request.Name, request.ParentFolderId, cancellationToken);
        if (existing != null)
            throw new ConflictException($"A folder named '{request.Name}' already exists in this location");

        var folder = new Folder
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ParentFolderId = request.ParentFolderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _folderRepository.CreateAsync(folder, cancellationToken);

        return new FolderResponse
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt
        };
    }

    public async Task<FolderResponse> UpdateAsync(Guid id, UpdateFolderRequest request, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Folder with ID {id} not found");

        // Check for circular reference if parent is changing
        if (request.ParentFolderId != folder.ParentFolderId)
        {
            var hasCircularRef = await _folderRepository.HasCircularReferenceAsync(id, request.ParentFolderId, cancellationToken);
            if (hasCircularRef)
                throw new ValidationException("Circular folder reference detected");

            // Validate new parent exists if specified
            if (request.ParentFolderId.HasValue)
            {
                var parentFolder = await _folderRepository.GetByIdAsync(request.ParentFolderId.Value, cancellationToken);
                if (parentFolder == null)
                    throw new NotFoundException($"Parent folder with ID {request.ParentFolderId.Value} not found");

                // Check depth limit
                var parentDepth = await _folderRepository.GetDepthAsync(request.ParentFolderId.Value, cancellationToken);
                if (parentDepth >= MaxDepth)
                    throw new ValidationException($"Maximum folder nesting depth ({MaxDepth}) exceeded");
            }
        }

        // Check for duplicate name if name or parent is changing
        if (request.Name.Trim() != folder.Name || request.ParentFolderId != folder.ParentFolderId)
        {
            var existing = await _folderRepository.GetByNameAndParentAsync(request.Name, request.ParentFolderId, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new ConflictException($"A folder named '{request.Name}' already exists in this location");
        }

        folder.Name = request.Name.Trim();
        folder.ParentFolderId = request.ParentFolderId;
        folder.UpdatedAt = DateTime.UtcNow;

        await _folderRepository.UpdateAsync(folder, cancellationToken);

        return new FolderResponse
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt,
            UpdatedAt = folder.UpdatedAt
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Folder with ID {id} not found");

        // Check if folder is empty (MVP approach: prevent deletion if not empty)
        var isEmpty = await _folderRepository.IsEmptyAsync(id, cancellationToken);
        if (!isEmpty)
            throw new ValidationException("Cannot delete folder containing templates or subfolders");

        await _folderRepository.DeleteAsync(id, cancellationToken);
    }
}
