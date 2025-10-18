using Microsoft.AspNetCore.Mvc;
using PromptTemplateManager.Application.DTOs;
using PromptTemplateManager.Application.Services;

namespace PromptTemplateManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FoldersController : ControllerBase
{
    private readonly IFolderService _folderService;

    public FoldersController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    /// <summary>
    /// Get folder tree (hierarchical structure)
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<FolderTreeResponse>> GetFolderTree(CancellationToken cancellationToken)
    {
        var tree = await _folderService.GetFolderTreeAsync(cancellationToken);
        return Ok(tree);
    }

    /// <summary>
    /// Get folder by ID with details
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FolderDetailsResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var folder = await _folderService.GetByIdAsync(id, cancellationToken);
        return Ok(folder);
    }

    /// <summary>
    /// Get children of a folder (or root folders if parentId is null)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FolderResponse>>> GetChildren([FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        var children = await _folderService.GetChildrenAsync(parentId, cancellationToken);
        return Ok(children);
    }

    /// <summary>
    /// Create a new folder
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FolderResponse>> Create([FromBody] CreateFolderRequest request, CancellationToken cancellationToken)
    {
        var folder = await _folderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = folder.Id }, folder);
    }

    /// <summary>
    /// Update a folder (rename or move)
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FolderResponse>> Update(Guid id, [FromBody] UpdateFolderRequest request, CancellationToken cancellationToken)
    {
        var folder = await _folderService.UpdateAsync(id, request, cancellationToken);
        return Ok(folder);
    }

    /// <summary>
    /// Delete an empty folder
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _folderService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
