namespace PromptTemplateManager.Application.DTOs;

public class UpdateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
}
