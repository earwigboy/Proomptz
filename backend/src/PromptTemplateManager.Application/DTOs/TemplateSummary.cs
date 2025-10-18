namespace PromptTemplateManager.Application.DTOs;

public class TemplateSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public string? FolderName { get; set; }
    public string ContentPreview { get; set; } = string.Empty;
    public int PlaceholderCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
