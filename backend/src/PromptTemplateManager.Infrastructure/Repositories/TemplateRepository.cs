using Microsoft.EntityFrameworkCore;
using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Core.Interfaces;
using PromptTemplateManager.Infrastructure.Data;

namespace PromptTemplateManager.Infrastructure.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly ApplicationDbContext _context;

    public TemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Templates
            .Include(t => t.Folder)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Template>> GetAllAsync(
        Guid? folderId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Templates
            .Include(t => t.Folder)
            .AsQueryable();

        if (folderId.HasValue)
        {
            query = query.Where(t => t.FolderId == folderId.Value);
        }
        else
        {
            query = query.Where(t => t.FolderId == null);
        }

        return await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(Guid? folderId, CancellationToken cancellationToken = default)
    {
        var query = _context.Templates.AsQueryable();

        if (folderId.HasValue)
        {
            query = query.Where(t => t.FolderId == folderId.Value);
        }
        else
        {
            query = query.Where(t => t.FolderId == null);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Template> CreateAsync(Template template, CancellationToken cancellationToken = default)
    {
        _context.Templates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<Template> UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        _context.Templates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.Templates.FindAsync(new object[] { id }, cancellationToken);
        if (template != null)
        {
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsWithNameInFolderAsync(
        string name,
        Guid? folderId,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Templates
            .Where(t => t.Name.ToLower() == name.ToLower());

        if (folderId.HasValue)
        {
            query = query.Where(t => t.FolderId == folderId.Value);
        }
        else
        {
            query = query.Where(t => t.FolderId == null);
        }

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
