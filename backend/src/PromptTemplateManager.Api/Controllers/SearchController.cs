using Microsoft.AspNetCore.Mvc;
using PromptTemplateManager.Application.DTOs;
using PromptTemplateManager.Core.Interfaces;

namespace PromptTemplateManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public SearchController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<TemplateListResponse>> SearchTemplates(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new
            {
                error = "ValidationError",
                message = "Search query is required",
                details = new[] { "Query parameter 'q' cannot be empty" }
            });
        }

        var (items, totalCount) = await _templateService.SearchTemplatesAsync(q, page, pageSize, cancellationToken);

        var response = new TemplateListResponse
        {
            Items = items.Select(t => new TemplateSummary
            {
                Id = t.Id,
                Name = t.Name,
                ContentPreview = t.Content.Length > 200 ? t.Content.Substring(0, 200) : t.Content,
                FolderId = t.FolderId,
                FolderName = t.Folder?.Name,
                PlaceholderCount = 0, // Could calculate if needed
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            HasMore = page * pageSize < totalCount
        };

        return Ok(response);
    }
}
