using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PromptTemplateManager.Application.DTOs;
using PromptTemplateManager.Core.Interfaces;
using System.Text.RegularExpressions;

namespace PromptTemplateManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IPlaceholderService _placeholderService;
    private readonly IDevinClient _devinClient;
    private readonly IValidator<CreateTemplateRequest> _createValidator;
    private readonly IValidator<UpdateTemplateRequest> _updateValidator;

    public TemplatesController(
        ITemplateService templateService,
        IPlaceholderService placeholderService,
        IDevinClient devinClient,
        IValidator<CreateTemplateRequest> createValidator,
        IValidator<UpdateTemplateRequest> updateValidator)
    {
        _templateService = templateService;
        _placeholderService = placeholderService;
        _devinClient = devinClient;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<TemplateListResponse>> GetTemplates(
        [FromQuery] Guid? folderId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(1, page);

        var (items, totalCount) = await _templateService.GetAllAsync(folderId, page, pageSize, cancellationToken);

        var summaries = items.Select(t => new TemplateSummary
        {
            Id = t.Id,
            Name = t.Name,
            FolderId = t.FolderId,
            FolderName = t.Folder?.Name,
            ContentPreview = t.Content.Length > 200 ? t.Content.Substring(0, 200) : t.Content,
            PlaceholderCount = CountPlaceholders(t.Content),
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        });

        return Ok(new TemplateListResponse
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            HasMore = totalCount > page * pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateResponse>> GetTemplate(Guid id, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetByIdAsync(id, cancellationToken);
        if (template == null)
        {
            return NotFound(new { error = "NotFound", message = $"Template with ID {id} not found" });
        }

        return Ok(new TemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Content = template.Content,
            FolderId = template.FolderId,
            FolderName = template.Folder?.Name,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<TemplateResponse>> CreateTemplate(
        [FromBody] CreateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                error = "ValidationError",
                message = "Validation failed",
                details = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var template = await _templateService.CreateAsync(
            request.Name,
            request.Content,
            request.FolderId,
            cancellationToken);

        var response = new TemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Content = template.Content,
            FolderId = template.FolderId,
            FolderName = template.Folder?.Name,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateResponse>> UpdateTemplate(
        Guid id,
        [FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                error = "ValidationError",
                message = "Validation failed",
                details = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var template = await _templateService.UpdateAsync(
            id,
            request.Name,
            request.Content,
            request.FolderId,
            cancellationToken);

        var response = new TemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Content = template.Content,
            FolderId = template.FolderId,
            FolderName = template.Folder?.Name,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTemplate(Guid id, CancellationToken cancellationToken)
    {
        await _templateService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id}/placeholders")]
    public async Task<ActionResult<PlaceholderListResponse>> GetPlaceholders(
        Guid id,
        CancellationToken cancellationToken)
    {
        var template = await _templateService.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound(new { message = $"Template with ID {id} not found" });

        var placeholderNames = _placeholderService.ExtractPlaceholderNames(template.Content ?? string.Empty);

        var response = new PlaceholderListResponse
        {
            TemplateId = template.Id.ToString(),
            TemplateName = template.Name,
            Placeholders = placeholderNames.Select(name => new PlaceholderInfo
            {
                Name = name,
                Description = null
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPost("{id}/generate")]
    public async Task<ActionResult<PromptInstanceResponse>> GeneratePrompt(
        Guid id,
        [FromBody] GeneratePromptRequest request,
        CancellationToken cancellationToken)
    {
        var template = await _templateService.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound(new { message = $"Template with ID {id} not found" });

        var requiredPlaceholders = _placeholderService.ExtractPlaceholderNames(template.Content ?? string.Empty);

        if (!_placeholderService.ValidatePlaceholderValues(requiredPlaceholders, request.PlaceholderValues))
        {
            return BadRequest(new
            {
                error = "ValidationError",
                message = "All placeholders must have values",
                requiredPlaceholders
            });
        }

        var generatedPrompt = _placeholderService.GeneratePrompt(template.Content ?? string.Empty, request.PlaceholderValues);

        var response = new PromptInstanceResponse
        {
            TemplateId = template.Id.ToString(),
            TemplateName = template.Name,
            GeneratedPrompt = generatedPrompt,
            PlaceholderValues = request.PlaceholderValues
        };

        return Ok(response);
    }

    [HttpPost("{id}/send")]
    public async Task<ActionResult<SendPromptResponse>> SendPrompt(
        Guid id,
        [FromBody] GeneratePromptRequest request,
        CancellationToken cancellationToken)
    {
        var template = await _templateService.GetByIdAsync(id, cancellationToken);
        if (template == null)
            return NotFound(new { message = $"Template with ID {id} not found" });

        var requiredPlaceholders = _placeholderService.ExtractPlaceholderNames(template.Content ?? string.Empty);

        if (!_placeholderService.ValidatePlaceholderValues(requiredPlaceholders, request.PlaceholderValues))
        {
            return BadRequest(new
            {
                error = "ValidationError",
                message = "All placeholders must have values",
                requiredPlaceholders
            });
        }

        var generatedPrompt = _placeholderService.GeneratePrompt(template.Content ?? string.Empty, request.PlaceholderValues);
        var (success, message, responseId) = await _devinClient.SendPromptAsync(generatedPrompt, cancellationToken);

        var response = new SendPromptResponse
        {
            Success = success,
            Message = message,
            DevinResponseId = responseId
        };

        return Ok(response);
    }

    private static int CountPlaceholders(string content)
    {
        var regex = new Regex(@"\{\{([a-zA-Z_][a-zA-Z0-9_]*)\}\}");
        var matches = regex.Matches(content);
        return matches.Select(m => m.Groups[1].Value).Distinct().Count();
    }
}
