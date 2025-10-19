using PromptTemplateManager.Core.Entities;
using PromptTemplateManager.Core.Exceptions;
using PromptTemplateManager.Core.Interfaces;

namespace PromptTemplateManager.Application.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templateRepository;

    public TemplateService(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _templateRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<(IEnumerable<Template> Items, int TotalCount)> GetAllAsync(
        Guid? folderId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var items = await _templateRepository.GetAllAsync(folderId, page, pageSize, cancellationToken);
        var count = await _templateRepository.GetCountAsync(folderId, cancellationToken);
        return (items, count);
    }

    public async Task<Template> CreateAsync(
        string name,
        string content,
        Guid? folderId,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate name in folder
        if (await _templateRepository.ExistsWithNameInFolderAsync(name, folderId, null, cancellationToken))
        {
            throw new ConflictException($"A template with the name '{name}' already exists in this folder");
        }

        var template = new Template
        {
            Id = Guid.NewGuid(),
            Name = name,
            Content = content,
            FolderId = folderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _templateRepository.CreateAsync(template, cancellationToken);
    }

    public async Task<Template> UpdateAsync(
        Guid id,
        string name,
        string content,
        Guid? folderId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException($"Template with ID {id} not found");
        }

        // Check for duplicate name in folder (excluding current template)
        if (await _templateRepository.ExistsWithNameInFolderAsync(name, folderId, id, cancellationToken))
        {
            throw new ConflictException($"A template with the name '{name}' already exists in this folder");
        }

        template.Name = name;
        template.Content = content;
        template.FolderId = folderId;
        template.UpdatedAt = DateTime.UtcNow;

        return await _templateRepository.UpdateAsync(template, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        if (template == null)
        {
            throw new NotFoundException($"Template with ID {id} not found");
        }

        await _templateRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<(IEnumerable<Template> Items, int TotalCount)> SearchTemplatesAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return (Enumerable.Empty<Template>(), 0);
        }

        var items = await _templateRepository.SearchTemplatesAsync(query, page, pageSize, cancellationToken);
        var totalCount = await _templateRepository.GetSearchCountAsync(query, cancellationToken);

        return (items, totalCount);
    }
}
