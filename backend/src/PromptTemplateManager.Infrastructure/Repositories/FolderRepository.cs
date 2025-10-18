using Microsoft.EntityFrameworkCore;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Core.Interfaces;
using PromptTemplateManager.Infrastructure.Data;

namespace PromptTemplateManager.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly ApplicationDbContext _context;

    public FolderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Include(f => f.ParentFolder)
            .Include(f => f.Templates)
            .Include(f => f.ChildFolders)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<List<Folder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Include(f => f.Templates)
            .Include(f => f.ChildFolders)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Folder>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Include(f => f.Templates)
            .Include(f => f.ChildFolders)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Folder>> GetChildrenAsync(Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Where(f => f.ParentFolderId == parentId)
            .Include(f => f.Templates)
            .Include(f => f.ChildFolders)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Folder?> GetByNameAndParentAsync(string name, Guid? parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .FirstOrDefaultAsync(f => f.Name.ToLower() == name.ToLower() && f.ParentFolderId == parentId, cancellationToken);
    }

    public async Task<bool> HasCircularReferenceAsync(Guid folderId, Guid? newParentId, CancellationToken cancellationToken = default)
    {
        if (newParentId == null)
            return false;

        // Check if the new parent is the folder itself
        if (newParentId == folderId)
            return true;

        // Check if the new parent is a descendant of the folder
        var currentParentId = newParentId;
        var visitedFolders = new HashSet<Guid>();

        while (currentParentId.HasValue)
        {
            if (currentParentId == folderId)
                return true;

            if (visitedFolders.Contains(currentParentId.Value))
                return true; // Circular reference detected in existing structure

            visitedFolders.Add(currentParentId.Value);

            var parentFolder = await _context.Folders
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == currentParentId.Value, cancellationToken);

            currentParentId = parentFolder?.ParentFolderId;
        }

        return false;
    }

    public async Task<int> GetDepthAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        var folder = await _context.Folders
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == folderId, cancellationToken);

        if (folder == null)
            return 0;

        var depth = 0;
        var currentParentId = folder.ParentFolderId;

        while (currentParentId.HasValue && depth < 20) // Safety limit
        {
            depth++;
            var parent = await _context.Folders
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == currentParentId.Value, cancellationToken);

            currentParentId = parent?.ParentFolderId;
        }

        return depth;
    }

    public async Task<bool> IsEmptyAsync(Guid folderId, CancellationToken cancellationToken = default)
    {
        var hasTemplates = await _context.Templates
            .AnyAsync(t => t.FolderId == folderId, cancellationToken);

        if (hasTemplates)
            return false;

        var hasSubfolders = await _context.Folders
            .AnyAsync(f => f.ParentFolderId == folderId, cancellationToken);

        return !hasSubfolders;
    }

    public async Task<Folder> CreateAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync(cancellationToken);
        return folder;
    }

    public async Task UpdateAsync(Folder folder, CancellationToken cancellationToken = default)
    {
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var folder = await _context.Folders.FindAsync(new object[] { id }, cancellationToken);
        if (folder != null)
        {
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
